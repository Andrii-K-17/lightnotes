using LightNotes.Application.Common;

namespace LightNotes.Application.Attachments;

/// <summary>
/// Attachment content and metadata returned by the service.
/// </summary>
public sealed record AttachmentFile(Stream Content, string ContentType, string Sha256);

public interface IAttachmentService
{
    /// <summary>
    /// Stores an attachment for an existing note.
    /// </summary>
    Task<Result> UploadAsync(
        Guid id,
        Guid noteId,
        Stream content,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Opens an attachment and returns its content and metadata.
    /// </summary>
    Task<Result<AttachmentFile>> OpenAsync(Guid id, CancellationToken cancellationToken);
}
