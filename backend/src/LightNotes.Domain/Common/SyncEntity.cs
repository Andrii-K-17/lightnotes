namespace LightNotes.Domain.Common;

/// <summary>Base for entities replicated between devices by the sync protocol.</summary>
public abstract class SyncEntity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>Incremented by the server on every applied change.</summary>
    public int Version { get; private set; }

    /// <summary>Position in the owner's change log.</summary>
    public long ServerSeq { get; private set; }

    /// <summary>Client time of creation.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Server time of the last change.</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Soft delete.</summary>
    public DateTimeOffset? DeletedAt { get; private set; }

    /// <summary>Permanent deletion.</summary>
    public DateTimeOffset? PurgedAt { get; private set; }

    public bool IsDeleted => DeletedAt is not null;

    public bool IsPurged => PurgedAt is not null;

    protected SyncEntity() { }

    protected SyncEntity(Guid id, Guid userId, DateTimeOffset createdAt)
    {
        Guard.ClientId(id);
        Guard.OwnerId(userId);

        Id = id;
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public void MarkChanged(long serverSeq, DateTimeOffset now)
    {
        Version++;
        ServerSeq = serverSeq;
        UpdatedAt = now;
    }

    public void Delete(DateTimeOffset now)
    {
        if (IsPurged)
        {
            return;
        }

        DeletedAt ??= now;
    }

    public void Restore()
    {
        EnsureNotPurged();
        DeletedAt = null;
    }

    public void Purge(DateTimeOffset now)
    {
        if (IsPurged)
        {
            return;
        }

        DeletedAt ??= now;
        PurgedAt = now;
        ClearContent();
    }

    protected abstract void ClearContent();

    protected void EnsureNotPurged()
    {
        if (IsPurged)
        {
            throw new DomainException(DomainErrorCodes.EntityPurged, "The entity was permanently deleted.");
        }
    }
}
