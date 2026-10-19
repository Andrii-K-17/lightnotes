using LightNotes.Domain.Common;
using LightNotes.Domain.Folders;
using Xunit;
using static LightNotes.Domain.Tests.TestData;

namespace LightNotes.Domain.Tests;

public class SyncEntityTests
{
    private static Folder NewFolder() => Folder.Create(NewId(), OwnerId, "Work", "a0", Now);

    [Fact]
    public void New_entity_starts_at_version_zero()
    {
        var folder = NewFolder();

        Assert.Equal(0, folder.Version);
        Assert.Equal(Now, folder.CreatedAt);
        Assert.Equal(Now, folder.UpdatedAt);
        Assert.False(folder.IsDeleted);
        Assert.False(folder.IsPurged);
    }

    [Fact]
    public void Create_rejects_ids_that_are_not_uuid_v7()
    {
        foreach (var id in new[] { Guid.Empty, Guid.NewGuid() })
        {
            AssertFails(DomainErrorCodes.InvalidId, () => Folder.Create(id, OwnerId, "Work", "a0", Now));
        }
    }

    [Fact]
    public void Create_rejects_empty_owner()
    {
        AssertFails(DomainErrorCodes.InvalidId, () => Folder.Create(NewId(), Guid.Empty, "Work", "a0", Now));
    }

    [Fact]
    public void MarkChanged_bumps_version_and_records_log_position()
    {
        var folder = NewFolder();
        var later = Now.AddMinutes(5);

        folder.MarkChanged(42, later);
        folder.MarkChanged(43, later.AddMinutes(1));

        Assert.Equal(2, folder.Version);
        Assert.Equal(43, folder.ServerSeq);
        Assert.Equal(later.AddMinutes(1), folder.UpdatedAt);
    }

    [Fact]
    public void Delete_is_idempotent_and_keeps_first_timestamp()
    {
        var folder = NewFolder();

        folder.Delete(Now.AddHours(1));
        folder.Delete(Now.AddHours(2));

        Assert.Equal(Now.AddHours(1), folder.DeletedAt);
    }

    [Fact]
    public void Restore_clears_deletion()
    {
        var folder = NewFolder();
        folder.Delete(Now);

        folder.Restore();

        Assert.False(folder.IsDeleted);
    }

    [Fact]
    public void Purge_drops_content_and_keeps_a_tombstone()
    {
        var folder = NewFolder();

        folder.Purge(Now.AddDays(30));

        Assert.True(folder.IsPurged);
        Assert.True(folder.IsDeleted);
        Assert.Equal("", folder.Name);
    }

    [Fact]
    public void Purged_entity_cannot_be_changed_or_restored_but_delete_is_a_no_op()
    {
        var folder = NewFolder();
        folder.Purge(Now);

        AssertFails(DomainErrorCodes.EntityPurged, folder.Restore);
        AssertFails(DomainErrorCodes.EntityPurged, () => folder.Rename("New"));

        folder.Delete(Now.AddDays(1));
        Assert.Equal(Now, folder.DeletedAt);
    }

    [Fact]
    public void Timestamps_are_normalized_to_utc()
    {
        var local = new DateTimeOffset(2026, 10, 10, 15, 0, 0, TimeSpan.FromHours(3));

        var folder = Folder.Create(NewId(), OwnerId, "Work", "a0", local);
        folder.MarkChanged(1, local);
        folder.Delete(local);

        Assert.Equal(TimeSpan.Zero, folder.CreatedAt.Offset);
        Assert.Equal(TimeSpan.Zero, folder.UpdatedAt.Offset);
        Assert.Equal(TimeSpan.Zero, folder.DeletedAt!.Value.Offset);
        Assert.Equal(local, folder.CreatedAt);
    }
}
