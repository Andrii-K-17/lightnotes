using LightNotes.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LightNotes.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("LIGHTNOTES_CONNECTION")
            ?? "Host=localhost;Database=lightnotes;Username=lightnotes;Password=lightnotes";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseLightNotesDatabase(connectionString);

        return new AppDbContext(optionsBuilder.Options, new DesignTimeUser());
    }

    private sealed class DesignTimeUser : ICurrentUser
    {
        public Guid UserId => Guid.Empty;
    }
}
