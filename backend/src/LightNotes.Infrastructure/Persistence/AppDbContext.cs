using LightNotes.Application.Abstractions;
using LightNotes.Domain.Attachments;
using LightNotes.Domain.Folders;
using LightNotes.Domain.Notes;
using LightNotes.Domain.Tags;
using LightNotes.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUser currentUser)
    : IdentityUserContext<ApplicationUser, Guid>(options),
        IAppDbContext
{
    private Guid CurrentUserId => currentUser.UserId;

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<Folder> Folders => Set<Folder>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<Attachment> Attachments => Set<Attachment>();

    internal DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasPostgresExtension("citext");

        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        builder.Entity<Note>().HasQueryFilter(QueryFilters.Tenant, n => n.UserId == CurrentUserId);
        builder
            .Entity<Folder>()
            .HasQueryFilter(QueryFilters.Tenant, f => f.UserId == CurrentUserId);

        builder.Entity<Tag>().HasQueryFilter(QueryFilters.Tenant, t => t.UserId == CurrentUserId);
        builder
            .Entity<Attachment>()
            .HasQueryFilter(QueryFilters.Tenant, a => a.UserId == CurrentUserId);
    }
}
