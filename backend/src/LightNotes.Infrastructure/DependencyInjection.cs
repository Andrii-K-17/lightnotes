using LightNotes.Application.Abstractions;
using LightNotes.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LightNotes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(
            (provider, options) =>
            {
                var connectionString =
                    provider.GetRequiredService<IConfiguration>().GetConnectionString("Default")
                    ?? throw new InvalidOperationException(
                        "ConnectionStrings:Default is not configured."
                    );

                options.UseLightNotesDatabase(connectionString);
            }
        );

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

        return services;
    }
}
