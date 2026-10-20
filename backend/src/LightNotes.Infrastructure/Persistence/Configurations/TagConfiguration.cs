using LightNotes.Domain.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightNotes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures database mapping, properties, and unique conditional indexes for the Tag entity.
/// </summary>
internal sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ConfigureSyncEntity("tags");

        builder.Property(t => t.Name).HasColumnType("citext");

        builder.HasIndex(t => new { t.UserId, t.Name }).IsUnique().HasFilter("deleted_at IS NULL");
    }
}
