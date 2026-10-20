using LightNotes.Domain.Folders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightNotes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures database mapping and properties for the Folder entity.
/// </summary>
internal sealed class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ConfigureSyncEntity("folders");

        builder.Property(f => f.Name).HasMaxLength(Folder.MaxNameLength);
        builder.Property(f => f.Position).HasMaxLength(Folder.MaxPositionLength);
    }
}
