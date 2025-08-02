namespace EasyReasy.FileStorage;

/// <summary>
/// Disposable implementation for removing file system watchers.
/// Can be used with any IWatchableFileSystem implementation.
/// </summary>
public class WatcherDisposable : IDisposable
{
    private readonly IWatchableFileSystem _fileSystem;
    private readonly IFileSystemWatcher _watcher;

    /// <summary>
    /// Initializes a new instance of the WatcherDisposable.
    /// </summary>
    /// <param name="fileSystem">The file system that manages the watcher.</param>
    /// <param name="watcher">The watcher to remove when disposed.</param>
    public WatcherDisposable(IWatchableFileSystem fileSystem, IFileSystemWatcher watcher)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
    }

    /// <summary>
    /// Removes the watcher from the file system asynchronously.
    /// </summary>
    public void Dispose()
    {
        _ = Task.Run(async () => await _fileSystem.RemoveFileSystemWatcher(_watcher));
    }
} 