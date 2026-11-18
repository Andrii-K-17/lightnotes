using System.Net.Http.Json;
using System.Net.ServerSentEvents;
using System.Runtime.CompilerServices;
using System.Text.Json;
using LightNotes.Application.Ai;
using LightNotes.Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LightNotes.Infrastructure.Ai;

internal sealed class OpenAiCompatibleClient(
    HttpClient http,
    IOptions<AiOptions> options,
    ILogger<OpenAiCompatibleClient> logger
) : IAiChatClient
{
    public async Task<Result<IAsyncEnumerable<string>>> StreamAsync(
        string system,
        string user,
        CancellationToken cancellationToken
    )
    {
        var ai = options.Value;

        var body = new Dictionary<string, object>
        {
            ["model"] = ai.Model,
            ["stream"] = true,
            ["temperature"] = 0.3,
            ["max_completion_tokens"] = ai.MaxOutputTokens,
            ["messages"] = new[]
            {
                new { role = "system", content = system },
                new { role = "user", content = user },
            },
        };

        if (ai.ReasoningEffort.Length > 0)
        {
            body["reasoning_effort"] = ai.ReasoningEffort;
            body["include_reasoning"] = false;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = JsonContent.Create(body),
        };

        HttpResponseMessage response;

        try
        {
            response = await http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
        }
        catch (Exception exception)
            when ((exception is HttpRequestException or TaskCanceledException)
                && !cancellationToken.IsCancellationRequested
            )
        {
            logger.LogWarning(exception, "The AI provider is unreachable");

            return Error.Unavailable("ai.provider_unavailable");
        }

        if (!response.IsSuccessStatusCode)
        {
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning(
                "The AI provider answered {Status}: {Detail}",
                (int)response.StatusCode,
                detail.Length > 300 ? detail[..300] : detail
            );
            response.Dispose();

            return Error.Unavailable("ai.provider_unavailable");
        }

        return Result<IAsyncEnumerable<string>>.FromValue(ReadAsync(response, cancellationToken));
    }

    private static async IAsyncEnumerable<string> ReadAsync(
        HttpResponseMessage response,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        using (response)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

            await foreach (var item in SseParser.Create(stream).EnumerateAsync(cancellationToken))
            {
                if (item.Data == "[DONE]")
                {
                    yield break;
                }

                var delta = ExtractDelta(item.Data);

                if (!string.IsNullOrEmpty(delta))
                {
                    yield return delta;
                }
            }
        }
    }

    /// <summary>
    /// The text of one streamed chunk;
    /// chunks without text (role, usage) give null.
    /// </summary>
    private static string? ExtractDelta(string json)
    {
        using var document = JsonDocument.Parse(json);

        return
            document.RootElement.TryGetProperty("choices", out var choices)
            && choices.GetArrayLength() > 0
            && choices[0].TryGetProperty("delta", out var delta)
            && delta.TryGetProperty("content", out var content)
            && content.ValueKind == JsonValueKind.String
            ? content.GetString()
            : null;
    }
}
