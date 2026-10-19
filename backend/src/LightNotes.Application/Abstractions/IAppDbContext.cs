using LightNotes.Domain.Attachments;
using LightNotes.Domain.Folders;
using LightNotes.Domain.Notes;
using LightNotes.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace LightNotes.Application.Abstractions;

/// <summary>The application's view of the database.</summary>
public interface IAppDbContext
{
    DbSet<Note> Notes { get; }

    DbSet<Folder> Folders { get; }

    DbSet<Tag> Tags { get; }

    DbSet<Attachment> Attachments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
