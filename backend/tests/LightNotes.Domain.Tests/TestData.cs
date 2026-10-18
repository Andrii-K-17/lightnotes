using LightNotes.Domain.Common;
using Xunit;

namespace LightNotes.Domain.Tests;

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 10, 17, 12, 0, 0, TimeSpan.Zero);

    public static readonly Guid OwnerId = Guid.NewGuid();

    public static Guid NewId() => Guid.CreateVersion7();

    public static void AssertFails(string code, Action action)
    {
        var exception = Assert.Throws<DomainException>(action);
        Assert.Equal(code, exception.Code);
    }
}
