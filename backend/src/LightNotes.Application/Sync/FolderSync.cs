using LightNotes.Domain.Common;
using LightNotes.Domain.Folders;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Application.Sync;

internal static class FolderSync
{
    /// <summary>
    /// Applies folder changes and returns a result for each change.
    /// </summary>
    public static async Task<List<FolderResult>> ApplyAsync(
        SyncContext context,
        IReadOnlyList<FolderChange> changes,
        CancellationToken cancellationToken
    )
    {
        var ids = changes.Select(change => change.Id).ToList();

        var existing = await context
            .Db.Folders.Where(f => ids.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, cancellationToken);
        var foreign = await context.ForeignIdsAsync(
            context.Db.Folders,
            [.. ids.Where(id => !existing.ContainsKey(id))],
            cancellationToken
        );

        var results = new List<FolderResult>(changes.Count);

        foreach (var change in changes)
        {
            if (foreign.Contains(change.Id))
            {
                results.Add(Rejected(change.Id, "sync.id_taken"));
                continue;
            }

            existing.TryGetValue(change.Id, out var folder);

            try
            {
                results.Add(Apply(context, change, folder));
            }
            catch (DomainException exception)
            {
                if (folder is not null)
                {
                    context.Db.Discard(folder);
                }

                results.Add(Rejected(change.Id, exception.Code));
            }
        }

        return results;
    }

    /// <summary>
    /// Creates or updates a folder from a single sync change.
    /// </summary>
    private static FolderResult Apply(SyncContext context, FolderChange change, Folder? folder)
    {
        if (folder is null)
        {
            folder = Folder.Create(
                change.Id,
                context.UserId,
                change.Name,
                change.Position,
                change.CreatedAt
            );

            if (change.DeletedAt is not null)
            {
                folder.Delete(context.Now);
            }

            folder.MarkChanged(context.Tx.NextSeq(), context.Now);
            context.Db.Folders.Add(folder);

            return Applied(folder);
        }

        var before = (folder.Name, folder.Position, folder.IsDeleted);

        folder.Rename(change.Name);
        folder.Reorder(change.Position);

        if (change.DeletedAt is not null)
        {
            folder.Delete(context.Now);
        }
        else if (folder.IsDeleted)
        {
            folder.Restore();
        }

        if (before != (folder.Name, folder.Position, folder.IsDeleted))
        {
            folder.MarkChanged(context.Tx.NextSeq(), context.Now);
        }

        return Applied(folder);
    }

    /// <summary>
    /// Creates a result for an applied folder change.
    /// </summary>
    private static FolderResult Applied(Folder folder) =>
        new(folder.Id, ChangeStatus.Applied, folder.Version, folder.ServerSeq);

    /// <summary>
    /// Creates a result for a rejected folder change.
    /// </summary>
    private static FolderResult Rejected(Guid id, string code) =>
        new(id, ChangeStatus.Rejected, 0, 0, code);
}
