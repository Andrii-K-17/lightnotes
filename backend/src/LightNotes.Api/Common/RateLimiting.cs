using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace LightNotes.Api.Common;

public static class RateLimitPolicies
{
    public const string Auth = "auth";
}

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services) =>
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(
                RateLimitPolicies.Auth,
                http =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                        }
                    )
            );

            options.OnRejected = async (context, cancellationToken) =>
            {
                var http = context.HttpContext;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    http.Response.Headers.RetryAfter = (
                        (int)Math.Ceiling(retryAfter.TotalSeconds)
                    ).ToString(CultureInfo.InvariantCulture);
                }

                http.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await http
                    .RequestServices.GetRequiredService<IProblemDetailsService>()
                    .WriteAsync(
                        new ProblemDetailsContext
                        {
                            HttpContext = http,
                            ProblemDetails = new ProblemDetails
                            {
                                Status = StatusCodes.Status429TooManyRequests,
                                Extensions = { ["code"] = "rate_limited" },
                            },
                        }
                    );
            };
        });
}
