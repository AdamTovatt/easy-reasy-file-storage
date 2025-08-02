namespace EasyReasy.FileStorage;

/// <summary>
/// Defines a watcher that can be notified of file system changes.
/// </summary>
public interface IFileSystemWatcher
{
    /// <summary>
    /// Called when a file system change occurs.
    /// </summary>
    /// <param name="change">The change event that occurred.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OnFileSystemChangedAsync(FileSystemChangeEvent change);
}