using LightNotes.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LightNotes.Api.Common;

/// <summary>
/// Intercepts and handles global application exceptions,
/// mapping them to structured Problem Details responses.
/// </summary>
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var (status, code) = exception is DomainException domain
            ? (StatusCodes.Status400BadRequest, domain.Code)
            : (StatusCodes.Status500InternalServerError, "internal_error");

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = status,
                    Extensions = { ["code"] = code },
                },
            }
        );
    }
}
