using LightNotes.Domain.Folders;
using LightNotes.Domain.Notes;
using LightNotes.Domain.Tags;

namespace LightNotes.Application.Sync;

internal static class SyncMapping
{
    public static FolderDto ToDto(this Folder f) =>
        new(f.Id, f.Version, f.ServerSeq, f.Name, f.Position, f.CreatedAt, f.DeletedAt, f.PurgedAt);

    public static TagDto ToDto(this Tag t) =>
        new(t.Id, t.Version, t.ServerSeq, t.Name, t.CreatedAt, t.DeletedAt, t.PurgedAt);

    public static NoteDto ToDto(this Note n) =>
        new(
            n.Id,
            n.Version,
            n.ServerSeq,
            n.FolderId,
            n.Title,
            n.Content,
            n.ContentText,
            n.Pinned,
            n.TagIds,
            n.CreatedAt,
            n.EditedAt,
            n.DeletedAt,
            n.PurgedAt
        );
}
