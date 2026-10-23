using System.Text;
using System.Text.Json.Serialization;
using LightNotes.Api.Common;
using LightNotes.Api.Endpoints;
using LightNotes.Application.Abstractions;
using LightNotes.Application.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

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

        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context =>
            {
                var problem = context.ProblemDetails;

                if (!problem.Extensions.ContainsKey("code"))
                {
                    problem.Extensions["code"] =
                        problem is HttpValidationProblemDetails
                            ? "validation.failed"
                            : $"http.{problem.Status}";
                }
            }
        );
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddValidation();
        services.AddOpenApi();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddAuthorization();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>(
                (bearer, jwtOptions) =>
                {
                    var jwt = jwtOptions.Value;

                    bearer.MapInboundClaims = false;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwt.SigningKey)
                        ),
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        ClockSkew = TimeSpan.FromSeconds(30),
                    };
                }
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

        var v1 = app.MapGroup("/api/v1");
        v1.MapAuthEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();
        }

        return app;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
