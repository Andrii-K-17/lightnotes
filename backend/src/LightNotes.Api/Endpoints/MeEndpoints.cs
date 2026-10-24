using LightNotes.Api.Common;
using LightNotes.Application.Auth;

namespace LightNotes.Api.Endpoints;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/me",
                async (IAuthService auth, CancellationToken ct) =>
                    (await auth.GetProfileAsync(ct)).ToHttpResult()
            )
            .WithTags("Me")
            .RequireAuthorization()
            .Produces<UserProfile>();

        return app;
    }
}
