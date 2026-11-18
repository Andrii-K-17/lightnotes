using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LightNotes.Api.Common;
using LightNotes.Application.Ai;

namespace LightNotes.Api.Endpoints;

public static class AiEndpoints
{
    public static IEndpointRouteBuilder MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/ai/actions",
                async Task<IResult> (
                    AiRequest request,
                    IAiService ai,
                    ILoggerFactory loggers,
                    CancellationToken ct
                ) =>
                {
                    var result = await ai.RunAsync(request, ct);

                    return result.IsSuccess
                        ? TypedResults.ServerSentEvents(
                            ToEvents(result.Value, loggers.CreateLogger("LightNotes.Ai"), ct)
                        )
                        : result.Error.ToProblem();
                }
            )
            .WithTags("AI")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Ai)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }

    /// <summary>
    /// Converts AI deltas into SSE delta, done, or error events.
    /// </summary>
    private static async IAsyncEnumerable<SseItem<string>> ToEvents(
        IAsyncEnumerable<string> deltas,
        ILogger logger,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await using var enumerator = deltas.GetAsyncEnumerator(cancellationToken);
        var failed = false;

        while (true)
        {
            string chunk;

            try
            {
                if (!await enumerator.MoveNextAsync())
                {
                    break;
                }

                chunk = enumerator.Current;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "The AI stream failed");
                failed = true;
                break;
            }

            yield return new SseItem<string>(JsonSerializer.Serialize(chunk), "delta");
        }

        yield return failed
            ? new SseItem<string>("\"ai.provider_unavailable\"", "error")
            : new SseItem<string>("null", "done");
    }
}
