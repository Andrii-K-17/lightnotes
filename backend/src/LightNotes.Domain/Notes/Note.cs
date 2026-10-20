using System.Text;
using System.Text.Json;
using LightNotes.Domain.Common;

namespace LightNotes.Domain.Notes;

public sealed class Note : SyncEntity
{
    public const int MaxTitleLength = 200;
    public const int MaxContentBytes = 1024 * 1024;
    public const int MaxTags = 20;
    public const string EmptyContent = "{}";

    public Guid? FolderId { get; private set; }

    public string Title { get; private set; } = "";

    /// <summary>Content as JSON.</summary>
    public string Content { get; private set; } = EmptyContent;

    /// <summary>Plain text of the document, used for search.</summary>
    public string ContentText { get; private set; } = "";

    public bool Pinned { get; private set; }

    /// <summary>Client time of the last user edit.</summary>
    public DateTimeOffset EditedAt { get; private set; }

    public List<Guid> TagIds { get; private set; } = [];

    private Note() { }

    private Note(Guid id, Guid userId, DateTimeOffset createdAt)
        : base(id, userId, createdAt)
    {
        EditedAt = CreatedAt;
    }

    public static Note Create(Guid id, Guid userId, DateTimeOffset createdAt) =>
        new(id, userId, createdAt);

    public void Edit(string title, string content, string contentText, DateTimeOffset editedAt)
    {
        EnsureNotPurged();

        if (title.Length > MaxTitleLength)
        {
            throw new DomainException(
                DomainErrorCodes.NoteTitleTooLong,
                $"Title is longer than {MaxTitleLength} characters."
            );
        }

        ValidateContent(content);

        if (Encoding.UTF8.GetByteCount(contentText) > MaxContentBytes)
        {
            throw new DomainException(
                DomainErrorCodes.NoteContentTooLarge,
                "Content text is too large."
            );
        }

        Title = title;
        Content = content;
        ContentText = contentText;
        EditedAt = editedAt.ToUniversalTime();
    }

    public void MoveTo(Guid? folderId)
    {
        EnsureNotPurged();
        RequireReference(folderId);
        FolderId = folderId;
    }

    public void SetPinned(bool pinned)
    {
        EnsureNotPurged();
        Pinned = pinned;
    }

    public void SetTags(IEnumerable<Guid> tagIds)
    {
        EnsureNotPurged();

        var distinct = tagIds.Distinct().ToList();

        if (distinct.Count > MaxTags)
        {
            throw new DomainException(
                DomainErrorCodes.NoteTooManyTags,
                $"A note can have at most {MaxTags} tags."
            );
        }

        foreach (var tagId in distinct)
        {
            RequireReference(tagId);
        }

        TagIds = distinct;
    }

    protected override void ClearContent()
    {
        FolderId = null;
        Title = "";
        Content = EmptyContent;
        ContentText = "";
        Pinned = false;
        TagIds = [];
    }

    private static void RequireReference(Guid? id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException(
                DomainErrorCodes.NoteInvalidReference,
                "Reference id is invalid."
            );
        }
    }

    private static void ValidateContent(string content)
    {
        if (Encoding.UTF8.GetByteCount(content) > MaxContentBytes)
        {
            throw new DomainException(
                DomainErrorCodes.NoteContentTooLarge,
                "Content is too large."
            );
        }

        try
        {
            using var document = JsonDocument.Parse(content);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new DomainException(
                    DomainErrorCodes.NoteContentInvalid,
                    "Content must be a JSON object."
                );
            }
        }
        catch (JsonException)
        {
            throw new DomainException(
                DomainErrorCodes.NoteContentInvalid,
                "Content is not valid JSON."
            );
        }
    }
}
