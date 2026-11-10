using LightNotes.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LightNotes.Infrastructure.Persistence;

internal sealed class SyncTransactions(AppDbContext db) : ISyncTransactions
{
    /// <summary>
    /// Starts a transaction and reserves sequence numbers for a user's sync changes.
    /// </summary>
    /// <returns>
    /// A transaction that assigns sequence numbers and commits the changes.
    /// </returns>
    public async Task<ISyncTransaction> BeginAsync(
        Guid userId,
        int reserve,
        CancellationToken cancellationToken
    )
    {
        var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await db
                .Users.Where(user => user.Id == userId)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(user => user.SyncSeq, user => user.SyncSeq + reserve),
                    cancellationToken
                );

            var last = await db
                .Users.Where(user => user.Id == userId)
                .Select(user => user.SyncSeq)
                .SingleAsync(cancellationToken);

            return new SyncTransaction(db, transaction, last - reserve + 1, last);
        }
        catch
        {
            await transaction.DisposeAsync();
            throw;
        }
    }

    private sealed class SyncTransaction(
        AppDbContext db,
        IDbContextTransaction inner,
        long first,
        long last
    ) : ISyncTransaction
    {
        private long _next = first;

        /// <summary>
        /// Returns the next reserved sequence number.
        /// </summary>
        public long NextSeq() =>
            _next <= last
                ? _next++
                : throw new InvalidOperationException(
                    "The reserved sequence numbers are exhausted."
                );

        /// <summary>
        /// Saves pending changes and commits the transaction.
        /// </summary>
        public async Task CommitAsync(CancellationToken cancellationToken)
        {
            await db.SaveChangesAsync(cancellationToken);
            await inner.CommitAsync(cancellationToken);
        }

        public ValueTask DisposeAsync() => inner.DisposeAsync();
    }
}
