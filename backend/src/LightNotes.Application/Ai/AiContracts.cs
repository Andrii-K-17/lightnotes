using LightNotes.Application.Common;

namespace LightNotes.Application.Ai;

/// <summary>
/// <para><c>Action</c> is "improve", "summarize", "translate" or "custom".</para>
/// <para><c>Language</c> is the target of a translation.</para>
/// <para><c>Instruction</c> is the user's own prompt for "custom".</para>
/// </summary>
public sealed record AiRequest(
    string Action,
    string Text,
    string? Language = null,
    string? Instruction = null
);

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
