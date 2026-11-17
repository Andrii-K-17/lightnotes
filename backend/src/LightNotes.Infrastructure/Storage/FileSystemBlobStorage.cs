using LightNotes.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace LightNotes.Infrastructure.Storage;

internal sealed class FileSystemBlobStorage(IOptions<StorageOptions> options) : IBlobStorage
{
    private readonly string _root = Path.GetFullPath(options.Value.Path);

    public async Task SaveAsync(string key, Stream content, CancellationToken cancellationToken)
    {
        var path = PathFor(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await using var file = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            81_920,
            useAsync: true
        );
        await content.CopyToAsync(file, cancellationToken);
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        var path = PathFor(key);
        Stream? stream = File.Exists(path)
            ? new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                81_920,
                useAsync: true
            )
            : null;

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        File.Delete(PathFor(key));

        return Task.CompletedTask;
    }

    private string PathFor(string key)
    {
        if (key.Length != 32 || !key.All(char.IsAsciiHexDigitLower))
        {
            throw new ArgumentException("Invalid storage key.", nameof(key));
        }

        return Path.Combine(_root, key[..2], key[2..4], key);
    }
}
