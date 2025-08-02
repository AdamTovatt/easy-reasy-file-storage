namespace EasyReasy.FileStorage;

/// <summary>
/// Defines a file system that supports watching for changes.
/// </summary>
public interface IWatchableFileSystem
{
    /// <summary>
    /// Adds a file system watcher that will be notified of changes.
    /// </summary>
    /// <param name="watcher">The watcher to add.</param>
    /// <returns>A disposable that can be used to remove the watcher.</returns>
    Task<IDisposable> AddFileSystemWatcher(IFileSystemWatcher watcher);

    /// <summary>
    /// Removes a file system watcher.
    /// </summary>
    /// <param name="watcher">The watcher to remove.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    Task RemoveFileSystemWatcher(IFileSystemWatcher watcher);
}