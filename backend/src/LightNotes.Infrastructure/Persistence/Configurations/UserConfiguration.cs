using LightNotes.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LightNotes.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures database mapping and unique indexes for the ApplicationUser entity.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users");

        builder
            .HasIndex(u => u.NormalizedUserName)
            .HasDatabaseName("ix_users_normalized_user_name")
            .IsUnique();
        builder
            .HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("ix_users_normalized_email")
            .IsUnique();
    }
}
