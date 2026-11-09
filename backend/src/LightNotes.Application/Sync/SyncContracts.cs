namespace LightNotes.Application.Sync;

public enum ChangeStatus
{
    Applied,
    Conflict,
    Merged,
    Rejected,
}

public static class SyncLimits
{
    public const int MaxChanges = 100;
    public const int MaxPullLimit = 500;

    /// <summary>Maximum note-text size per pull page.</summary>
    public const int MaxPullCharacters = 4_000_000;
}

// ---- Push ----

public sealed record PushRequest(
    IReadOnlyList<FolderChange> Folders,
    IReadOnlyList<TagChange> Tags,
    IReadOnlyList<NoteChange> Notes
);

public sealed record FolderChange(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Name,
    string Position,
    DateTimeOffset? DeletedAt = null
);

public sealed record TagChange(
    Guid Id,
    DateTimeOffset CreatedAt,
    string Name,
    DateTimeOffset? DeletedAt = null
);

/// <summary>Note title and content, included only when edited.</summary>
public sealed record NoteContentChange(
    string Title,
    string Content,
    string ContentText,
    DateTimeOffset EditedAt
);

/// <summary>Note metadata, included only when changed.</summary>
public sealed record NoteMetaChange(
    Guid? FolderId,
    IReadOnlyList<Guid> TagIds,
    bool Pinned,
    DateTimeOffset? DeletedAt = null,
    DateTimeOffset? PurgedAt = null
);

public sealed record NoteChange(
    Guid Id,
    int BaseVersion,
    DateTimeOffset CreatedAt,
    NoteContentChange? Content = null,
    NoteMetaChange? Meta = null
);

// ---- Entities returned to the client ----

public sealed record FolderDto(
    Guid Id,
    int Version,
    long ServerSeq,
    string Name,
    string Position,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DeletedAt,
    DateTimeOffset? PurgedAt
);

public sealed record TagDto(
    Guid Id,
    int Version,
    long ServerSeq,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? DeletedAt,
    DateTimeOffset? PurgedAt
);

public sealed record NoteDto(
    Guid Id,
    int Version,
    long ServerSeq,
    Guid? FolderId,
    string Title,
    string Content,
    string ContentText,
    bool Pinned,
    IReadOnlyList<Guid> TagIds,
    DateTimeOffset CreatedAt,
    DateTimeOffset EditedAt,
    DateTimeOffset? DeletedAt,
    DateTimeOffset? PurgedAt
);

// ---- Results ----

public sealed record PushResponse(
    IReadOnlyList<FolderResult> Folders,
    IReadOnlyList<TagResult> Tags,
    IReadOnlyList<NoteResult> Notes
);

public sealed record FolderResult(
    Guid Id,
    ChangeStatus Status,
    int Version,
    long ServerSeq,
    string? Code = null
);

public sealed record TagResult(
    Guid Id,
    ChangeStatus Status,
    int Version,
    long ServerSeq,
    string? Code = null,
    Guid? MergedInto = null,
    TagDto? Current = null
);

public sealed record NoteResult(
    Guid Id,
    ChangeStatus Status,
    int Version,
    long ServerSeq,
    string? Code = null,
    Guid? ConflictCopyId = null,
    NoteDto? Current = null
);

// ---- Pull ----

public sealed record PullResponse(
    IReadOnlyList<FolderDto> Folders,
    IReadOnlyList<TagDto> Tags,
    IReadOnlyList<NoteDto> Notes,
    long NextSeq,
    bool HasMore
);
