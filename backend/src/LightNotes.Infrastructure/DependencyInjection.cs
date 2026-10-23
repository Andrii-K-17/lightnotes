using LightNotes.Application.Abstractions;
using LightNotes.Application.Auth;
using LightNotes.Infrastructure.Identity;
using LightNotes.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
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

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = AuthRules.PasswordMinLength;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>();

        services.Configure<PasswordHasherOptions>(options => options.IterationCount = 210_000);

        services.AddScoped<IAuthService, AuthService>();

        services
            .AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<TokenService>();

        return services;
    }

    /// <summary>
    /// Asynchronously applies any pending database migrations using a temporary service scope.
    /// </summary>
    public static async Task MigrateDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default
    )
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(cancellationToken);
    }
}
