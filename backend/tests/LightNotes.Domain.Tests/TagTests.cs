using LightNotes.Domain.Common;
using LightNotes.Domain.Tags;
using Xunit;
using static LightNotes.Domain.Tests.TestData;

namespace LightNotes.Domain.Tests;

public class TagTests
{
    [Fact]
    public void Create_trims_the_name()
    {
        var tag = Tag.Create(NewId(), OwnerId, "  ideas ", Now);

        Assert.Equal("ideas", tag.Name);
    }

    [Fact]
    public void Name_is_required()
    {
        AssertFails(
            DomainErrorCodes.TagNameRequired,
            () => Tag.Create(NewId(), OwnerId, "  ", Now)
        );
    }

    [Fact]
    public void Name_is_limited_to_40_characters()
    {
        var tag = Tag.Create(NewId(), OwnerId, new string('x', Tag.MaxNameLength), Now);

        AssertFails(
            DomainErrorCodes.TagNameTooLong,
            () => tag.Rename(new string('x', Tag.MaxNameLength + 1))
        );
    }
}
