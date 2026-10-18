using LightNotes.Domain.Common;
using LightNotes.Domain.Notes;
using Xunit;
using static LightNotes.Domain.Tests.TestData;

namespace LightNotes.Domain.Tests;

public class NoteTests
{
    private const string ValidContent = """{"type":"doc","content":[]}""";

    private static Note NewNote() => Note.Create(NewId(), OwnerId, Now);

    [Fact]
    public void New_note_is_empty()
    {
        var note = NewNote();

        Assert.Equal("", note.Title);
        Assert.Equal(Note.EmptyContent, note.Content);
        Assert.Equal(Now, note.EditedAt);
        Assert.Null(note.FolderId);
        Assert.False(note.Pinned);
        Assert.Empty(note.TagIds);
    }

    [Fact]
    public void Edit_stores_the_new_state()
    {
        var note = NewNote();
        var editedAt = Now.AddMinutes(3);

        note.Edit("Plan", ValidContent, "plain text", editedAt);

        Assert.Equal("Plan", note.Title);
        Assert.Equal(ValidContent, note.Content);
        Assert.Equal("plain text", note.ContentText);
        Assert.Equal(editedAt, note.EditedAt);
    }

    [Fact]
    public void Title_is_optional_but_limited_to_200_characters()
    {
        var note = NewNote();

        note.Edit("", ValidContent, "", Now);
        note.Edit(new string('x', Note.MaxTitleLength), ValidContent, "", Now);

        AssertFails(DomainErrorCodes.NoteTitleTooLong,
            () => note.Edit(new string('x', Note.MaxTitleLength + 1), ValidContent, "", Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("[]")]
    [InlineData("\"text\"")]
    public void Content_must_be_a_json_object(string content)
    {
        AssertFails(DomainErrorCodes.NoteContentInvalid, () => NewNote().Edit("", content, "", Now));
    }

    [Fact]
    public void Content_is_limited_to_one_megabyte()
    {
        var tooLarge = "{\"t\":\"" + new string('x', Note.MaxContentBytes) + "\"}";

        AssertFails(DomainErrorCodes.NoteContentTooLarge, () => NewNote().Edit("", tooLarge, "", Now));
    }

    [Fact]
    public void Content_text_is_limited_to_one_megabyte()
    {
        var tooLarge = new string('x', Note.MaxContentBytes + 1);

        AssertFails(DomainErrorCodes.NoteContentTooLarge, () => NewNote().Edit("", ValidContent, tooLarge, Now));
    }

    [Fact]
    public void Failed_edit_leaves_the_note_unchanged()
    {
        var note = NewNote();
        note.Edit("Before", ValidContent, "before", Now);

        Assert.Throws<DomainException>(() => note.Edit("After", "broken", "after", Now.AddMinutes(1)));

        Assert.Equal("Before", note.Title);
        Assert.Equal(Now, note.EditedAt);
    }

    [Fact]
    public void MoveTo_sets_and_clears_the_folder()
    {
        var note = NewNote();
        var folderId = NewId();

        note.MoveTo(folderId);
        Assert.Equal(folderId, note.FolderId);

        note.MoveTo(null);
        Assert.Null(note.FolderId);
    }

    [Fact]
    public void MoveTo_rejects_an_empty_id()
    {
        AssertFails(DomainErrorCodes.NoteInvalidReference, () => NewNote().MoveTo(Guid.Empty));
    }

    [Fact]
    public void SetTags_removes_duplicates()
    {
        var note = NewNote();
        var tag = NewId();

        note.SetTags([tag, tag]);

        Assert.Equal([tag], note.TagIds);
    }

    [Fact]
    public void SetTags_allows_up_to_20_tags()
    {
        var note = NewNote();

        note.SetTags(Enumerable.Range(0, Note.MaxTags).Select(_ => NewId()));
        Assert.Equal(Note.MaxTags, note.TagIds.Count);

        AssertFails(DomainErrorCodes.NoteTooManyTags,
            () => note.SetTags(Enumerable.Range(0, Note.MaxTags + 1).Select(_ => NewId())));
    }

    [Fact]
    public void SetTags_rejects_an_empty_id()
    {
        AssertFails(DomainErrorCodes.NoteInvalidReference, () => NewNote().SetTags([Guid.Empty]));
    }

    [Fact]
    public void Trash_and_restore_keep_the_content()
    {
        var note = NewNote();
        note.Edit("Plan", ValidContent, "text", Now);

        note.Delete(Now.AddHours(1));
        Assert.True(note.IsDeleted);

        note.Restore();
        Assert.False(note.IsDeleted);
        Assert.Equal("Plan", note.Title);
    }

    [Fact]
    public void Purge_drops_everything_but_the_tombstone()
    {
        var note = NewNote();
        note.Edit("Plan", ValidContent, "text", Now);
        note.SetPinned(true);
        note.SetTags([NewId()]);
        note.MoveTo(NewId());

        note.Purge(Now.AddDays(30));

        Assert.True(note.IsPurged);
        Assert.True(note.IsDeleted);
        Assert.Equal("", note.Title);
        Assert.Equal(Note.EmptyContent, note.Content);
        Assert.Equal("", note.ContentText);
        Assert.False(note.Pinned);
        Assert.Null(note.FolderId);
        Assert.Empty(note.TagIds);
    }

    [Fact]
    public void Purged_note_cannot_be_edited()
    {
        var note = NewNote();
        note.Purge(Now);

        AssertFails(DomainErrorCodes.EntityPurged, () => note.Edit("", ValidContent, "", Now));
        AssertFails(DomainErrorCodes.EntityPurged, () => note.SetPinned(true));
    }
}
