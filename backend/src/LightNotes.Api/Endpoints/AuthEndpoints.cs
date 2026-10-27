using System.Security.Claims;
using LightNotes.Api.Common;
using LightNotes.Api.Contracts;
using LightNotes.Application.Auth;
using LightNotes.Application.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;

namespace LightNotes.Api.Endpoints;

public static class AuthEndpoints
{
    private const string SpaCallbackPath = "/auth/callback";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth")
            .RequireRateLimiting(RateLimitPolicies.Auth);

        group
            .MapPost(
                "/register",
                async (RegisterRequest request, IAuthService auth, CancellationToken ct) =>
                    (await auth.RegisterAsync(request.Email, request.Password, ct)).ToHttpResult()
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group
            .MapPost(
                "/login",
                async (
                    LoginRequest request,
                    IAuthService auth,
                    HttpContext http,
                    TimeProvider time,
                    CancellationToken ct
                ) =>
                    ToSessionResult(
                        await auth.LoginAsync(request.Email, request.Password, ct),
                        http,
                        time
                    )
            )
            .Produces<AccessTokenResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/refresh",
                async (
                    IAuthService auth,
                    HttpContext http,
                    TimeProvider time,
                    CancellationToken ct
                ) =>
                {
                    var token = RefreshCookie.Read(http.Request);

                    Result<AuthSession> result = token is null
                        ? Error.Unauthorized("auth.no_refresh_token")
                        : await auth.RefreshAsync(token, ct);

                    if (!result.IsSuccess)
                    {
                        RefreshCookie.Delete(http.Response);
                    }

                    return ToSessionResult(result, http, time);
                }
            )
            .RequireXRequestedWith()
            .Produces<AccessTokenResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group
            .MapPost(
                "/logout",
                async (IAuthService auth, HttpContext http, CancellationToken ct) =>
                {
                    await auth.LogoutAsync(RefreshCookie.Read(http.Request), ct);
                    RefreshCookie.Delete(http.Response);

                    return Results.NoContent();
                }
            )
            .RequireXRequestedWith()
            .Produces(StatusCodes.Status204NoContent);

        group
            .MapGet(
                "/google/login",
                async Task<IResult> (
                    IAuthenticationSchemeProvider schemes,
                    IConfiguration configuration
                ) =>
                {
                    if (await schemes.GetSchemeAsync(GoogleDefaults.AuthenticationScheme) is null)
                    {
                        return Results.Redirect(
                            SpaCallback(configuration, "auth.google_not_configured")
                        );
                    }

                    return TypedResults.Challenge(
                        new AuthenticationProperties
                        {
                            RedirectUri = "/api/v1/auth/google/callback",
                        },
                        [GoogleDefaults.AuthenticationScheme]
                    );
                }
            )
            .ExcludeFromDescription();

        group
            .MapGet(
                "/google/callback",
                async Task<IResult> (
                    IAuthService auth,
                    HttpContext http,
                    IConfiguration configuration,
                    CancellationToken ct
                ) =>
                {
                    var external = await http.AuthenticateAsync(ExternalCookie.Scheme);
                    await http.SignOutAsync(ExternalCookie.Scheme);

                    var subject = external.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                    var email = external.Principal?.FindFirstValue(ClaimTypes.Email);

                    if (!external.Succeeded || subject is null || email is null)
                    {
                        return Results.Redirect(SpaCallback(configuration, "auth.google_failed"));
                    }

                    var verified = string.Equals(
                        external.Principal!.FindFirstValue("email_verified"),
                        "true",
                        StringComparison.OrdinalIgnoreCase
                    );

                    var result = await auth.SignInWithGoogleAsync(
                        new GoogleIdentity(subject, email, verified),
                        ct
                    );

                    if (!result.IsSuccess)
                    {
                        return Results.Redirect(SpaCallback(configuration, result.Error.Code));
                    }

                    RefreshCookie.Append(
                        http.Response,
                        result.Value.RefreshToken,
                        result.Value.RefreshTokenExpiresAt
                    );

                    return Results.Redirect(SpaCallback(configuration));
                }
            )
            .ExcludeFromDescription();

        return app;
    }

    /// <summary>
    /// Sets the refresh cookie and returns the access token, or the failure as ProblemDetails.
    /// </summary>
    private static IResult ToSessionResult(
        Result<AuthSession> result,
        HttpContext http,
        TimeProvider time
    )
    {
        if (!result.IsSuccess)
        {
            return result.Error.ToProblem();
        }

        var session = result.Value;
        RefreshCookie.Append(http.Response, session.RefreshToken, session.RefreshTokenExpiresAt);

        var expiresIn = (int)(session.AccessTokenExpiresAt - time.GetUtcNow()).TotalSeconds;

        return TypedResults.Ok(new AccessTokenResponse(session.AccessToken, expiresIn));
    }

    private static string SpaCallback(IConfiguration configuration, string? error = null)
    {
        var frontendUrl = configuration["Frontend:BaseUrl"];

        if (
            !Uri.TryCreate(frontendUrl, UriKind.Absolute, out var frontendUri)
            || (frontendUri.Scheme != Uri.UriSchemeHttp && frontendUri.Scheme != Uri.UriSchemeHttps)
        )
        {
            throw new InvalidOperationException(
                "Configuration value 'Frontend:BaseUrl' must be an absolute HTTP or HTTPS URL."
            );
        }

        var callbackUri = new UriBuilder(frontendUri)
        {
            Path = SpaCallbackPath,
            Query = error is null ? string.Empty : $"error={Uri.EscapeDataString(error)}",
            Fragment = string.Empty,
        };

        return callbackUri.Uri.AbsoluteUri;
    }
}
