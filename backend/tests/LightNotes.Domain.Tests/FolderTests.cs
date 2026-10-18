using LightNotes.Domain.Common;
using LightNotes.Domain.Folders;
using Xunit;
using static LightNotes.Domain.Tests.TestData;

namespace LightNotes.Domain.Tests;

public class FolderTests
{
    [Fact]
    public void Create_trims_the_name()
    {
        var folder = Folder.Create(NewId(), OwnerId, "  Work  ", "a0", Now);

        Assert.Equal("Work", folder.Name);
        Assert.Equal("a0", folder.Position);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_is_required(string name)
    {
        AssertFails(DomainErrorCodes.FolderNameRequired, () => Folder.Create(NewId(), OwnerId, name, "a0", Now));
    }

    [Fact]
    public void Name_is_limited_to_60_characters()
    {
        var folder = Folder.Create(NewId(), OwnerId, new string('x', Folder.MaxNameLength), "a0", Now);
        Assert.Equal(Folder.MaxNameLength, folder.Name.Length);

        AssertFails(DomainErrorCodes.FolderNameTooLong, () => folder.Rename(new string('x', Folder.MaxNameLength + 1)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Position_is_required(string position)
    {
        AssertFails(DomainErrorCodes.FolderPositionInvalid, () => Folder.Create(NewId(), OwnerId, "Work", position, Now));
    }

    [Fact]
    public void Position_is_limited_in_length()
    {
        var folder = Folder.Create(NewId(), OwnerId, "Work", "a0", Now);

        AssertFails(DomainErrorCodes.FolderPositionInvalid, () => folder.Reorder(new string('a', Folder.MaxPositionLength + 1)));
    }
}
