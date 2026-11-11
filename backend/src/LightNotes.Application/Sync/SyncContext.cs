using LightNotes.Application.Abstractions;
using LightNotes.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Application.Sync;

internal sealed record SyncContext(
    IAppDbContext Db,
    Guid UserId,
    DateTimeOffset Now,
    ISyncTransaction Tx
)
{
    /// <summary>
    /// Ids that exist but belong to another account.
    /// </summary>
    public async Task<HashSet<Guid>> ForeignIdsAsync<T>(
        DbSet<T> set,
        IReadOnlyCollection<Guid> missingIds,
        CancellationToken cancellationToken
    )
        where T : SyncEntity
    {
        if (missingIds.Count == 0)
        {
            return [];
        }

        var ids = await set.IgnoreQueryFilters([QueryFilters.Tenant])
            .Where(entity => missingIds.Contains(entity.Id))
            .Select(entity => entity.Id)
            .ToListAsync(cancellationToken);

        return [.. ids];
    }
}
