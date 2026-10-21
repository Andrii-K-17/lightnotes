using System.Text.Json.Serialization;
using LightNotes.Application.Abstractions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace LightNotes.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter())
        );

        return services;
    }

    public static WebApplication MapApi(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => false })
            .ExcludeFromDescription();
        app.MapHealthChecks(
                "/ready",
                new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") }
            )
            .ExcludeFromDescription();

        return app;
    }
}
