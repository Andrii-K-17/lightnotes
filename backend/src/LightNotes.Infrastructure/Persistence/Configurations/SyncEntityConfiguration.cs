using LightNotes.Domain.Common;
using LightNotes.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightNotes.Infrastructure.Persistence.Configurations;

internal static class SyncEntityConfiguration
{
    /// <summary>
    /// Configures the base database mapping and relationships for synced entities.
    /// </summary>
    public static void ConfigureSyncEntity<T>(this EntityTypeBuilder<T> builder, string tableName)
        where T : SyncEntity
    {
        builder.ToTable(tableName);

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Version).IsConcurrencyToken();

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.ServerSeq });
    }
}
