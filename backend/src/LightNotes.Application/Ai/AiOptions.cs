using System.ComponentModel.DataAnnotations;

namespace LightNotes.Application.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Any OpenAI-compatible API.
    /// </summary>
    [Required]
    public string BaseUrl { get; init; } = "https://api.groq.com/openai/v1";

    public string ApiKey { get; init; } = "";

    [Required]
    public string Model { get; init; } = "openai/gpt-oss-20b";

    public string ReasoningEffort { get; init; } = "low";

    public int MaxInputCharacters { get; init; } = 20_000;

    public int MaxOutputTokens { get; init; } = 4096;
}
