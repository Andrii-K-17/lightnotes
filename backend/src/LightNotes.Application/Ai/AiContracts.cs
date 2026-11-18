using LightNotes.Application.Common;

namespace LightNotes.Application.Ai;

/// <summary>
/// <c>Action</c> is "improve", "summarize" or "translate".
/// <c>Language</c> is the target of a translation.
/// </summary>
public sealed record AiRequest(string Action, string Text, string? Language = null);

public interface IAiService
{
    /// <summary>
    /// Checks the request and starts the reply; the text arrives piece by piece.
    /// </summary>
    Task<Result<IAsyncEnumerable<string>>> RunAsync(
        AiRequest request,
        CancellationToken cancellationToken
    );
}

public interface IAiChatClient
{
    /// <summary>
    /// Sends the request and returns the reply as it is generated.
    /// </summary>
    Task<Result<IAsyncEnumerable<string>>> StreamAsync(
        string system,
        string user,
        CancellationToken cancellationToken
    );
}
