using Microsoft.EntityFrameworkCore;

namespace LightNotes.Infrastructure.Persistence;

internal static class DbContextOptionsExtensions
{
    public static DbContextOptionsBuilder UseLightNotesDatabase(
        this DbContextOptionsBuilder builder,
        string connectionString
    ) => builder.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();
}
