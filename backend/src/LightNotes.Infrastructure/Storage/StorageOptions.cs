using System.ComponentModel.DataAnnotations;

namespace LightNotes.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    /// <summary>
    /// Directory for uploaded files; relative paths start at the working directory.
    /// </summary>
    [Required]
    public string Path { get; init; } = "";
}
