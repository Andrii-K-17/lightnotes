using LightNotes.Application.Abstractions;
using LightNotes.Application.Common;
using LightNotes.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Application.Sync;

internal sealed class SyncService(
    IAppDbContext db,
    ICurrentUser currentUser,
    ISyncTransactions transactions,
    TimeProvider timeProvider
) : ISyncService
{
    /// <summary>
    /// Validates and applies client changes, then returns their results.
    /// </summary>
    public async Task<Result<PushResponse>> PushAsync(
        PushRequest request,
        CancellationToken cancellationToken
    )
    {
        var total = request.Folders.Count + request.Tags.Count + request.Notes.Count;

        if (total > SyncLimits.MaxChanges)
        {
            return Error.Validation("sync.too_many_changes");
        }

        var ids = request
            .Folders.Select(c => c.Id)
            .Concat(request.Tags.Select(c => c.Id))
            .Concat(request.Notes.Select(c => c.Id));

        if (ids.Distinct().Count() != total)
        {
            return Error.Validation("sync.duplicate_id");
        }

        if (total == 0)
        {
            return new PushResponse([], [], []);
        }

        await using var transaction = await transactions.BeginAsync(
            currentUser.UserId,
            total * 2,
            cancellationToken
        );
        var context = new SyncContext(
            db,
            currentUser.UserId,
            timeProvider.GetUtcNow(),
            transaction
        );

        var folders = await FolderSync.ApplyAsync(context, request.Folders, cancellationToken);
        var tags = await TagSync.ApplyAsync(context, request.Tags, cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        var notes = await NoteSync.ApplyAsync(context, request.Notes, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new PushResponse(folders, tags, notes);
    }

    /// <summary>
    /// Returns the next page of changes after the specified sequence number.
    /// </summary>
    public async Task<Result<PullResponse>> PullAsync(
        long since,
        int limit,
        CancellationToken cancellationToken
    )
    {
        if (since < 0 || limit < 1 || limit > SyncLimits.MaxPullLimit)
        {
            return Error.Validation("sync.invalid_cursor");
        }

        var folders = await db
            .Folders.AsNoTracking()
            .Where(f => f.ServerSeq > since)
            .OrderBy(f => f.ServerSeq)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);
        var tags = await db
            .Tags.AsNoTracking()
            .Where(t => t.ServerSeq > since)
            .OrderBy(t => t.ServerSeq)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);
        var notes = await db
            .Notes.AsNoTracking()
            .Where(n => n.ServerSeq > since)
            .OrderBy(n => n.ServerSeq)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var merged = folders
            .Cast<SyncEntity>()
            .Concat(tags)
            .Concat(notes)
            .OrderBy(e => e.ServerSeq)
            .ToList();

        var page = new List<SyncEntity>();
        var characters = 0;

        foreach (var entity in merged)
        {
            var size = entity is Domain.Notes.Note note
                ? note.Content.Length + note.ContentText.Length
                : 0;

            if (
                page.Count >= limit
                || (page.Count > 0 && characters + size > SyncLimits.MaxPullCharacters)
            )
            {
                break;
            }

            page.Add(entity);
            characters += size;
        }

        return new PullResponse(
            [.. page.OfType<Domain.Folders.Folder>().Select(f => f.ToDto())],
            [.. page.OfType<Domain.Tags.Tag>().Select(t => t.ToDto())],
            [.. page.OfType<Domain.Notes.Note>().Select(n => n.ToDto())],
            page.Count > 0 ? page[^1].ServerSeq : since,
            page.Count < merged.Count
        );
    }
}
