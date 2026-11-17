namespace LightNotes.Application.Attachments;

public static class ImageSniffer
{
    private static ReadOnlySpan<byte> Jpeg => [0xFF, 0xD8, 0xFF];

    private static ReadOnlySpan<byte> Png => [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>
    /// The media type of a supported image, or null.
    /// SVG and everything else is refused.
    /// </summary>
    public static string? Detect(ReadOnlySpan<byte> data)
    {
        if (data.StartsWith(Jpeg))
        {
            return "image/jpeg";
        }

        if (data.StartsWith(Png))
        {
            return "image/png";
        }

        if (data.StartsWith("GIF87a"u8) || data.StartsWith("GIF89a"u8))
        {
            return "image/gif";
        }

        if (
            data.Length >= 12
            && data[..4].SequenceEqual("RIFF"u8)
            && data.Slice(8, 4).SequenceEqual("WEBP"u8)
        )
        {
            return "image/webp";
        }

        return null;
    }
}
