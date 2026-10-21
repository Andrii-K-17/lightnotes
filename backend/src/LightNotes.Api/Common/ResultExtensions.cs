using LightNotes.Application.Common;

namespace LightNotes.Api.Common;

/// <summary>
/// Provides extension methods to map application results and errors to HTTP responses.
/// </summary>
internal static class ResultExtensions
{
    public static IResult ToHttpResult<T>(
        this Result<T> result,
        Func<T, IResult>? onSuccess = null
    ) =>
        result.IsSuccess
            ? onSuccess?.Invoke(result.Value) ?? TypedResults.Ok(result.Value)
            : result.Error.ToProblem();

    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();

    public static IResult ToProblem(this Error error) =>
        TypedResults.Problem(
            statusCode: StatusCodeOf(error.Type),
            extensions: new Dictionary<string, object?> { ["code"] = error.Code }
        );

    private static int StatusCodeOf(ErrorType type) =>
        type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.TooLarge => StatusCodes.Status413PayloadTooLarge,
            ErrorType.RateLimited => StatusCodes.Status429TooManyRequests,
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError,
        };
}
