namespace LightNotes.Application.Abstractions;

public interface IBlobStorage
{
    /// <summary>
    /// Saves content under the specified key.
    /// </summary>
    Task SaveAsync(string key, Stream content, CancellationToken cancellationToken);

    /// <summary>
    /// Opens content for reading, or returns null if the key does not exist.
    /// </summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes content; does nothing if the key does not exist.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken);
}
