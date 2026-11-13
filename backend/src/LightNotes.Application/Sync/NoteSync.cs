using System.Globalization;
using LightNotes.Domain.Common;
using LightNotes.Domain.Notes;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Application.Sync;

internal static class NoteSync
{
    /// <summary>
    /// Applies note changes and returns a result for each change.
    /// </summary>
    public static async Task<List<NoteResult>> ApplyAsync(
        SyncContext context,
        IReadOnlyList<NoteChange> changes,
        CancellationToken cancellationToken
    )
    {
        var ids = changes.Select(change => change.Id).ToList();

        var existing = await context
            .Db.Notes.Where(n => ids.Contains(n.Id))
            .ToDictionaryAsync(n => n.Id, cancellationToken);
        var foreign = await context.ForeignIdsAsync(
            context.Db.Notes,
            [.. ids.Where(id => !existing.ContainsKey(id))],
            cancellationToken
        );
        var references = await References.LoadAsync(context, changes, cancellationToken);

        var results = new List<NoteResult>(changes.Count);

        foreach (var change in changes)
        {
            if (foreign.Contains(change.Id))
            {
                results.Add(Rejected(change.Id, "sync.id_taken"));
                continue;
            }

            existing.TryGetValue(change.Id, out var note);
            var created = new List<Note>();

            try
            {
                results.Add(Apply(context, change, note, references, created));
            }
            catch (DomainException exception)
            {
                foreach (var entity in created.Append(note))
                {
                    if (entity is not null)
                    {
                        context.Db.Discard(entity);
                    }
                }

                results.Add(Rejected(change.Id, exception.Code));
            }
        }

        return results;
    }

    /// <summary>
    /// Creates or updates a note while resolving references and conflicts.
    /// </summary>
    private static NoteResult Apply(
        SyncContext context,
        NoteChange change,
        Note? note,
        References references,
        List<Note> created
    )
    {
        var folderId = references.Folder(change.Meta?.FolderId);
        var tagIds = references.Tags(change.Meta?.TagIds ?? []);

        if (tagIds.Count > Note.MaxTags)
        {
            throw new DomainException(DomainErrorCodes.NoteTooManyTags, "Too many tags.");
        }

        if (note is null)
        {
            return Create(context, change, folderId, tagIds, created);
        }

        if (note.IsPurged)
        {
            return change.Content is { } lost
                ? Conflict(note, CopyOf(context, lost, folderId, tagIds, created).Id)
                : Applied(note);
        }

        return Update(context, change, note, folderId, tagIds, created);
    }

    /// <summary>
    /// Creates a note from a sync change.
    /// </summary>
    private static NoteResult Create(
        SyncContext context,
        NoteChange change,
        Guid? folderId,
        List<Guid> tagIds,
        List<Note> created
    )
    {
        var note = Note.Create(change.Id, context.UserId, change.CreatedAt);

        if (change.Content is { } content)
        {
            note.Edit(content.Title, content.Content, content.ContentText, content.EditedAt);
        }

        if (change.Meta is { } meta)
        {
            ApplyMeta(context, note, meta, folderId, tagIds, keepAlive: false);
        }

        note.MarkChanged(context.Tx.NextSeq(), context.Now);
        note.MarkContentChanged();

        context.Db.Notes.Add(note);
        created.Add(note);

        return Applied(note);
    }

    /// <summary>
    /// Updates a note and preserves conflicting client edits as a copy.
    /// </summary>
    private static NoteResult Update(
        SyncContext context,
        NoteChange change,
        Note note,
        Guid? folderId,
        List<Guid> tagIds,
        List<Note> created
    )
    {
        var before = Snapshot.Of(note);
        var serverEditedSinceBase = note.ContentVersion > change.BaseVersion;
        Guid? copyId = null;
        var contentApplied = false;

        if (change.Content is { } content && !SameContent(note, content))
        {
            if (serverEditedSinceBase)
            {
                copyId = CopyOf(context, content, folderId, tagIds, created).Id;
            }
            else
            {
                note.Edit(content.Title, content.Content, content.ContentText, content.EditedAt);
                contentApplied = true;
            }
        }

        if (change.Meta is { } meta)
        {
            ApplyMeta(context, note, meta, folderId, tagIds, keepAlive: serverEditedSinceBase);
        }
        else if (contentApplied && note.IsDeleted)
        {
            note.Restore();
        }

        var after = Snapshot.Of(note);

        if (after != before)
        {
            note.MarkChanged(context.Tx.NextSeq(), context.Now);

            if (
                after.Title != before.Title
                || after.Content != before.Content
                || after.ContentText != before.ContentText
            )
            {
                note.MarkContentChanged();
            }
        }

        return copyId is null ? Applied(note) : Conflict(note, copyId.Value);
    }

    /// <summary>
    /// Applies folder, pin, tag, and deletion metadata to a note.
    /// </summary>
    private static void ApplyMeta(
        SyncContext context,
        Note note,
        NoteMetaChange meta,
        Guid? folderId,
        List<Guid> tagIds,
        bool keepAlive
    )
    {
        note.MoveTo(folderId);
        note.SetPinned(meta.Pinned);
        note.SetTags(tagIds);

        if (meta.PurgedAt is not null)
        {
            note.Purge(context.Now);
        }
        else if (meta.DeletedAt is not null)
        {
            if (!keepAlive)
            {
                note.Delete(context.Now);
            }
        }
        else if (note.IsDeleted)
        {
            note.Restore();
        }
    }

    /// <summary>
    /// Creates a separate note to preserve conflicting client content.
    /// </summary>
    private static Note CopyOf(
        SyncContext context,
        NoteContentChange content,
        Guid? folderId,
        List<Guid> tagIds,
        List<Note> created
    )
    {
        var stamp = content.EditedAt.UtcDateTime.ToString(
            "dd.MM.yyyy HH:mm",
            CultureInfo.InvariantCulture
        );
        var suffix = $"(conflict {stamp})";
        var room = Math.Max(0, Note.MaxTitleLength - suffix.Length - 1);
        var title = content.Title.Length > room ? content.Title[..room] : content.Title;

        var copy = Note.Create(Guid.CreateVersion7(), context.UserId, context.Now);
        copy.Edit(
            $"{title} {suffix}".Trim(),
            content.Content,
            content.ContentText,
            content.EditedAt
        );
        copy.MoveTo(folderId);
        copy.SetTags(tagIds);
        copy.MarkChanged(context.Tx.NextSeq(), context.Now);
        copy.MarkContentChanged();

        context.Db.Notes.Add(copy);
        created.Add(copy);

        return copy;
    }

    /// <summary>
    /// Checks whether the note's content matches the incoming content.
    /// </summary>
    private static bool SameContent(Note note, NoteContentChange content) =>
        note.Title == content.Title
        && note.Content == content.Content
        && note.ContentText == content.ContentText;

    /// <summary>
    /// Creates a result for an applied note change.
    /// </summary>
    private static NoteResult Applied(Note note) =>
        new(note.Id, ChangeStatus.Applied, note.Version, note.ServerSeq);

    /// <summary>
    /// Creates a result for a note change with a preserved conflict copy.
    /// </summary>
    private static NoteResult Conflict(Note note, Guid copyId) =>
        new(
            note.Id,
            ChangeStatus.Conflict,
            note.Version,
            note.ServerSeq,
            ConflictCopyId: copyId,
            Current: note.ToDto()
        );

    /// <summary>
    /// Creates a result for a rejected note change.
    /// </summary>
    private static NoteResult Rejected(Guid id, string code) =>
        new(id, ChangeStatus.Rejected, 0, 0, code);

    /// <summary>
    /// What a sync change can alter.
    /// </summary>
    private readonly record struct Snapshot(
        string Title,
        string Content,
        string ContentText,
        Guid? FolderId,
        bool Pinned,
        bool Deleted,
        string Tags
    )
    {
        /// <summary>
        /// Captures the note fields used to detect changes.
        /// </summary>
        public static Snapshot Of(Note n) =>
            new(
                n.Title,
                n.Content,
                n.ContentText,
                n.FolderId,
                n.Pinned,
                n.IsDeleted,
                string.Join(',', n.TagIds.Order())
            );
    }

    /// <summary>
    /// Folders and tags the pushed notes may point to: live and owned by the user.
    /// </summary>
    private sealed class References(HashSet<Guid> folders, HashSet<Guid> tags)
    {
        /// <summary>
        /// Loads the live folders and tags referenced by the incoming changes.
        /// </summary>
        public static async Task<References> LoadAsync(
            SyncContext context,
            IReadOnlyList<NoteChange> changes,
            CancellationToken cancellationToken
        )
        {
            var metas = changes.Where(c => c.Meta is not null).Select(c => c.Meta!).ToList();
            var folderIds = metas.Select(m => m.FolderId).OfType<Guid>().Distinct().ToList();
            var tagIds = metas.SelectMany(m => m.TagIds).Distinct().ToList();

            var liveFolders = await context
                .Db.Folders.Where(f => folderIds.Contains(f.Id) && f.DeletedAt == null)
                .Select(f => f.Id)
                .ToListAsync(cancellationToken);
            var liveTags = await context
                .Db.Tags.Where(t => tagIds.Contains(t.Id) && t.DeletedAt == null)
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            return new References([.. liveFolders], [.. liveTags]);
        }

        /// <summary>
        /// Returns the folder ID only if it refers to a live folder.
        /// </summary>
        public Guid? Folder(Guid? id) => id is { } value && folders.Contains(value) ? value : null;

        /// <summary>
        /// Returns distinct IDs for tags that are live.
        /// </summary>
        public List<Guid> Tags(IEnumerable<Guid> ids) => [.. ids.Distinct().Where(tags.Contains)];
    }
}
