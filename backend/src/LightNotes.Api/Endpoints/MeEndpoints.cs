using LightNotes.Api.Common;
using LightNotes.Api.Contracts;
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

        app.MapPut(
                "/me/ai-consent",
                async (AiConsentRequest request, IAuthService auth, CancellationToken ct) =>
                    (await auth.SetAiConsentAsync(request.Granted, ct)).ToHttpResult()
            )
            .WithTags("Me")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        return app;
    }
}
