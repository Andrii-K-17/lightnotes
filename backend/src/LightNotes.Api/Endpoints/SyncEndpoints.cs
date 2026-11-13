using LightNotes.Api.Common;
using LightNotes.Application.Sync;

namespace LightNotes.Api.Endpoints;

public static class SyncEndpoints
{
    public static IEndpointRouteBuilder MapSyncEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sync")
            .WithTags("Sync")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Sync);

        group
            .MapPost(
                "/push",
                async (PushRequest request, ISyncService sync, CancellationToken ct) =>
                    (await sync.PushAsync(request, ct)).ToHttpResult()
            )
            .Produces<PushResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group
            .MapGet(
                "/pull",
                async (long? since, int? limit, ISyncService sync, CancellationToken ct) =>
                    (
                        await sync.PullAsync(since ?? 0, limit ?? SyncLimits.MaxPullLimit, ct)
                    ).ToHttpResult()
            )
            .Produces<PullResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }
}
