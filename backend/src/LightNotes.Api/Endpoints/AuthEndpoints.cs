using LightNotes.Api.Common;
using LightNotes.Api.Contracts;
using LightNotes.Application.Auth;
using LightNotes.Application.Common;

namespace LightNotes.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Auth");

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
}
