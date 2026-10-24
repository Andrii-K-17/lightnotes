using LightNotes.Application.Common;

namespace LightNotes.Api.Common;

internal static class CsrfGuard
{
    private const string HeaderName = "X-Requested-With";

    public static RouteHandlerBuilder RequireXRequestedWith(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter(
            async (context, next) =>
                context.HttpContext.Request.Headers.ContainsKey(HeaderName)
                    ? await next(context)
                    : Error.Forbidden("auth.csrf_header_missing").ToProblem()
        );
}
