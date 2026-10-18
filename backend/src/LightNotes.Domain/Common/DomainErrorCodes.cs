namespace LightNotes.Domain.Common;

/// <summary>Stable error codes.</summary>
public static class DomainErrorCodes
{
    public const string InvalidId = "common.invalid_id";
    public const string EntityPurged = "common.entity_purged";

    public const string FolderNameRequired = "folder.name_required";
    public const string FolderNameTooLong = "folder.name_too_long";
    public const string FolderPositionInvalid = "folder.position_invalid";

    public const string TagNameRequired = "tag.name_required";
    public const string TagNameTooLong = "tag.name_too_long";

    public const string NoteTitleTooLong = "note.title_too_long";
    public const string NoteContentInvalid = "note.content_invalid";
    public const string NoteContentTooLarge = "note.content_too_large";
    public const string NoteTooManyTags = "note.too_many_tags";
    public const string NoteInvalidReference = "note.invalid_reference";
}
