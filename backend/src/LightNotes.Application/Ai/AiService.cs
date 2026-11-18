using LightNotes.Application.Auth;
using LightNotes.Application.Common;
using Microsoft.Extensions.Options;

namespace LightNotes.Application.Ai;

internal sealed class AiService(
    IOptions<AiOptions> options,
    IAuthService auth,
    IAiChatClient client
) : IAiService
{
    public async Task<Result<IAsyncEnumerable<string>>> RunAsync(
        AiRequest request,
        CancellationToken cancellationToken
    )
    {
        var ai = options.Value;

        if (!ai.Enabled || string.IsNullOrWhiteSpace(ai.ApiKey))
        {
            return Error.Unavailable("ai.disabled");
        }

        var profile = await auth.GetProfileAsync(cancellationToken);

        if (!profile.IsSuccess)
        {
            return profile.Error;
        }

        if (!profile.Value.AiConsent)
        {
            return Error.Forbidden("ai.consent_required");
        }

        var text = request.Text.Trim();

        if (text.Length == 0)
        {
            return Error.Validation("ai.empty_input");
        }

        if (text.Length > ai.MaxInputCharacters)
        {
            return Error.TooLarge("ai.input_too_large");
        }

        var system = AiPrompts.System(request.Action, request.Language);

        if (system is null)
        {
            return Error.Validation("ai.invalid_request");
        }

        return await client.StreamAsync(system, AiPrompts.User(text), cancellationToken);
    }
}
