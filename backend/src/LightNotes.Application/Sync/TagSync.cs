using LightNotes.Domain.Common;
using LightNotes.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Application.Sync;

internal static class TagSync
{
    /// <summary>
    /// Applies tag changes and returns a result for each change.
    /// </summary>
    public static async Task<List<TagResult>> ApplyAsync(
        SyncContext context,
        IReadOnlyList<TagChange> changes,
        CancellationToken cancellationToken
    )
    {
        var ids = changes.Select(change => change.Id).ToList();

        var existing = await context
            .Db.Tags.Where(t => ids.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, cancellationToken);
        var foreign = await context.ForeignIdsAsync(
            context.Db.Tags,
            ids.Where(id => !existing.ContainsKey(id)).ToList(),
            cancellationToken
        );

        var live = (
            await context.Db.Tags.Where(t => t.DeletedAt == null).ToListAsync(cancellationToken)
        ).ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        var results = new List<TagResult>(changes.Count);

        foreach (var change in changes)
        {
            if (foreign.Contains(change.Id))
            {
                results.Add(Rejected(change.Id, "sync.id_taken"));
                continue;
            }

            existing.TryGetValue(change.Id, out var tag);

            try
            {
                results.Add(Apply(context, change, tag, live));
            }
            catch (DomainException exception)
            {
                if (tag is not null)
                {
                    context.Db.Discard(tag);
                }

                results.Add(Rejected(change.Id, exception.Code));
            }
        }

        return results;
    }

    /// <summary>
    /// Creates or updates a tag, merging or rejecting duplicate names as needed.
    /// </summary>
    private static TagResult Apply(
        SyncContext context,
        TagChange change,
        Tag? tag,
        Dictionary<string, Tag> live
    )
    {
        var name = change.Name.Trim();
        var clash = live.GetValueOrDefault(name);
        var becomesLive = change.DeletedAt is null;

        if (tag is null)
        {
            if (becomesLive && clash is not null)
            {
                return new TagResult(
                    change.Id,
                    ChangeStatus.Merged,
                    0,
                    0,
                    MergedInto: clash.Id,
                    Current: clash.ToDto()
                );
            }

            tag = Tag.Create(change.Id, context.UserId, name, change.CreatedAt);

            if (!becomesLive)
            {
                tag.Delete(context.Now);
            }

            tag.MarkChanged(context.Tx.NextSeq(), context.Now);
            context.Db.Tags.Add(tag);

            if (becomesLive)
            {
                live[tag.Name] = tag;
            }

            return Applied(tag);
        }

        if (becomesLive && clash is not null && clash.Id != tag.Id)
        {
            return new TagResult(
                tag.Id,
                ChangeStatus.Rejected,
                0,
                0,
                "tag.name_taken",
                Current: tag.ToDto()
            );
        }

        var before = (tag.Name, tag.IsDeleted);

        live.Remove(tag.Name);
        tag.Rename(name);

        if (becomesLive)
        {
            if (tag.IsDeleted)
            {
                tag.Restore();
            }

            live[tag.Name] = tag;
        }
        else
        {
            tag.Delete(context.Now);
        }

        if (before != (tag.Name, tag.IsDeleted))
        {
            tag.MarkChanged(context.Tx.NextSeq(), context.Now);
        }

        return Applied(tag);
    }

    /// <summary>
    /// Creates a result for an applied tag change.
    /// </summary>
    private static TagResult Applied(Tag tag) =>
        new(tag.Id, ChangeStatus.Applied, tag.Version, tag.ServerSeq);

    /// <summary>
    /// Creates a result for a rejected tag change.
    /// </summary>
    private static TagResult Rejected(Guid id, string code) =>
        new(id, ChangeStatus.Rejected, 0, 0, code);
}
