namespace LightNotes.Application.Abstractions;

public interface ISyncTransactions
{
    /// <summary>Locks the user's sync counter and reserves sequence numbers.</summary>
    Task<ISyncTransaction> BeginAsync(
        Guid userId,
        int reserve,
        CancellationToken cancellationToken
    );
}

/// <summary>Disposing without committing rolls back the transaction.</summary>
public interface ISyncTransaction : IAsyncDisposable
{
    /// <summary>Returns the next reserved sequence number.</summary>
    long NextSeq();

    /// <summary>Saves pending changes and commits.</summary>
    Task CommitAsync(CancellationToken cancellationToken);
}
