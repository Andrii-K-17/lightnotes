using LightNotes.Domain.Attachments;
using LightNotes.Domain.Notes;
using LightNotes.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightNotes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures database mapping, properties, relationships, and indexes for the Attachment entity.
/// </summary>
internal sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("attachments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.ContentType).HasMaxLength(50);
        builder.Property(a => a.StorageKey).HasMaxLength(Attachment.MaxStorageKeyLength);
        builder.Property(a => a.Sha256).HasMaxLength(64);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne<Note>()
            .WithMany()
            .HasForeignKey(a => a.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.UserId, a.NoteId });
    }
}
