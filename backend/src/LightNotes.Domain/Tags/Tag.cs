using LightNotes.Domain.Common;

namespace LightNotes.Domain.Tags;

public sealed class Tag : SyncEntity
{
    public const int MaxNameLength = 40;

    /// <summary>Unique per user ignoring case among non-deleted tags.</summary>
    public string Name { get; private set; } = "";

    private Tag() { }

    private Tag(Guid id, Guid userId, DateTimeOffset createdAt) : base(id, userId, createdAt) { }

    public static Tag Create(Guid id, Guid userId, string name, DateTimeOffset createdAt)
    {
        var tag = new Tag(id, userId, createdAt);
        tag.Rename(name);
        return tag;
    }

    public void Rename(string name)
    {
        EnsureNotPurged();
        Name = Guard.Name(name, MaxNameLength, DomainErrorCodes.TagNameRequired, DomainErrorCodes.TagNameTooLong);
    }

    protected override void ClearContent() => Name = "";
}
