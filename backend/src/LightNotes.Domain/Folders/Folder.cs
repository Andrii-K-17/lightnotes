using System.Text;
using LightNotes.Domain.Common;

namespace LightNotes.Domain.Folders;

public sealed class Folder : SyncEntity
{
    public const int MaxNameLength = 60;
    public const int MaxPositionLength = 64;

    public string Name { get; private set; } = "";

    /// <summary>Defines the manual order of folders.</summary>
    public string Position { get; private set; } = "";

    private Folder() { }

    private Folder(Guid id, Guid userId, DateTimeOffset createdAt)
        : base(id, userId, createdAt) { }

    public static Folder Create(
        Guid id,
        Guid userId,
        string name,
        string position,
        DateTimeOffset createdAt
    )
    {
        var folder = new Folder(id, userId, createdAt);
        folder.Rename(name);
        folder.Reorder(position);
        return folder;
    }

    public void Rename(string name)
    {
        EnsureNotPurged();
        Name = Guard.Name(
            name,
            MaxNameLength,
            DomainErrorCodes.FolderNameRequired,
            DomainErrorCodes.FolderNameTooLong
        );
    }

    public void Reorder(string position)
    {
        EnsureNotPurged();

        if (string.IsNullOrWhiteSpace(position) || position.Length > MaxPositionLength)
        {
            throw new DomainException(
                DomainErrorCodes.FolderPositionInvalid,
                "Position is invalid."
            );
        }

        Position = position;
    }

    protected override void ClearContent()
    {
        Name = "";
        Position = "";
    }
}
