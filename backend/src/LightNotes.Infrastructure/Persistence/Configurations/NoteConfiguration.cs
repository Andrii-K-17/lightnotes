using LightNotes.Domain.Folders;
using LightNotes.Domain.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightNotes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures database mapping, properties, relationships, and filtered indexes for the Note entity.
/// </summary>
internal sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ConfigureSyncEntity("notes");

        builder.Property(n => n.Title).HasMaxLength(Note.MaxTitleLength);
        builder.Property(n => n.Content).HasColumnType("jsonb");

        builder
            .HasOne<Folder>()
            .WithMany()
            .HasForeignKey(n => n.FolderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => new { n.UserId, n.FolderId }).HasFilter("deleted_at IS NULL");

        builder
            .HasIndex(n => n.DeletedAt)
            .HasFilter("deleted_at IS NOT NULL AND purged_at IS NULL");
    }
}
