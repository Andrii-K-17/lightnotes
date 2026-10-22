using LightNotes.Api.Common;
using LightNotes.Api.Contracts;
using LightNotes.Application.Auth;

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

        return app;
    }
}
