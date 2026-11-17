using LightNotes.Api.Common;
using LightNotes.Application.Attachments;
using Microsoft.Net.Http.Headers;

namespace LightNotes.Api.Endpoints;

public static class AttachmentEndpoints
{
    public static IEndpointRouteBuilder MapAttachmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/attachments")
            .WithTags("Attachments")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.Sync);

        group
            .MapPut(
                "/{id:guid}",
                async (
                    Guid id,
                    Guid noteId,
                    HttpRequest request,
                    IAttachmentService attachments,
                    CancellationToken ct
                ) => (await attachments.UploadAsync(id, noteId, request.Body, ct)).ToHttpResult()
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group
            .MapGet(
                "/{id:guid}",
                async (
                    Guid id,
                    IAttachmentService attachments,
                    HttpContext http,
                    CancellationToken ct
                ) =>
                {
                    var result = await attachments.OpenAsync(id, ct);

                    if (!result.IsSuccess)
                    {
                        return result.Error.ToProblem();
                    }

                    http.Response.Headers.CacheControl = "private, max-age=31536000, immutable";

                    return Results.Stream(
                        result.Value.Content,
                        result.Value.ContentType,
                        entityTag: new EntityTagHeaderValue($"\"{result.Value.Sha256}\"")
                    );
                }
            )
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
