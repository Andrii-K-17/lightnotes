using System.Security.Cryptography;
using LightNotes.Application.Abstractions;
using LightNotes.Application.Common;
using LightNotes.Domain.Attachments;
using LightNotes.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Application.Attachments;

internal sealed class AttachmentService(
    IAppDbContext db,
    ICurrentUser currentUser,
    IBlobStorage blobs,
    TimeProvider timeProvider
) : IAttachmentService
{
    private const long QuotaBytes = 1L << 30;

    public async Task<Result> UploadAsync(
        Guid id,
        Guid noteId,
        Stream content,
        CancellationToken cancellationToken
    )
    {
        if (await db.Attachments.AnyAsync(a => a.Id == id, cancellationToken))
        {
            return Result.Success;
        }

        if (
            await db
                .Attachments.IgnoreQueryFilters([QueryFilters.Tenant])
                .AnyAsync(a => a.Id == id, cancellationToken)
        )
        {
            return Error.Conflict("attachment.id_taken");
        }

        if (!await db.Notes.AnyAsync(n => n.Id == noteId && n.PurgedAt == null, cancellationToken))
        {
            return Error.NotFound("attachment.note_not_found");
        }

        var bytes = await ReadAsync(content, cancellationToken);

        if (bytes is null)
        {
            return Error.TooLarge("attachment.too_large");
        }

        var contentType = ImageSniffer.Detect(bytes);

        if (contentType is null)
        {
            return Error.Validation("attachment.type_not_allowed");
        }

        var used = await db.Attachments.SumAsync(a => a.SizeBytes, cancellationToken);

        if (used + bytes.Length > QuotaBytes)
        {
            return Error.Forbidden("attachment.quota_exceeded");
        }

        var key = Guid.CreateVersion7().ToString("N");
        Attachment attachment;

        try
        {
            attachment = Attachment.Create(
                id,
                currentUser.UserId,
                noteId,
                contentType,
                bytes.Length,
                key,
                Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
                timeProvider.GetUtcNow()
            );
        }
        catch (DomainException exception)
        {
            return Error.Validation(exception.Code);
        }

        await blobs.SaveAsync(key, new MemoryStream(bytes), cancellationToken);

        try
        {
            db.Attachments.Add(attachment);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await blobs.DeleteAsync(key, CancellationToken.None);
            throw;
        }

        return Result.Success;
    }

    public async Task<Result<AttachmentFile>> OpenAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var attachment = await db
            .Attachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        var stream = attachment is null
            ? null
            : await blobs.OpenReadAsync(attachment.StorageKey, cancellationToken);

        return attachment is null || stream is null
            ? Error.NotFound("attachment.not_found")
            : new AttachmentFile(stream, attachment.ContentType, attachment.Sha256);
    }

    /// <summary>
    /// Reads the whole body, or returns null as soon as it exceeds the size limit.
    /// </summary>
    private static async Task<byte[]?> ReadAsync(
        Stream content,
        CancellationToken cancellationToken
    )
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[81_920];
        int read;

        while ((read = await content.ReadAsync(chunk, cancellationToken)) > 0)
        {
            buffer.Write(chunk, 0, read);

            if (buffer.Length > Attachment.MaxSizeBytes)
            {
                return null;
            }
        }

        return buffer.ToArray();
    }
}
