using System.Text;

namespace EasyReasy.FileStorage
{
    /// <summary>
    /// Local file system implementation that provides file operations relative to a base path.
    /// Includes security measures to prevent directory traversal attacks.
    /// </summary>
    public class LocalFileSystem : IWatchableFileSystem
    {
        private readonly string _basePath;
        private readonly List<IFileSystemWatcher> _watchers = new List<IFileSystemWatcher>();
        private readonly SemaphoreSlim _watchersSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the LocalFileSystem with the specified base path.
        /// </summary>
        /// <param name="basePath">The base path that all operations will be relative to. Use empty string for absolute paths.</param>
        public LocalFileSystem(string basePath)
        {
            _basePath = basePath ?? string.Empty;
        }

        /// <summary>
        /// Combines the base path with the provided path and validates for security.
        /// </summary>
        /// <param name="path">The relative path to combine.</param>
        /// <returns>The full absolute path.</returns>
        /// <exception cref="ArgumentException">Thrown when the path contains invalid characters or attempts directory traversal.</exception>
        private string GetFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            // Normalize path separators
            string normalizedPath = path.Replace('\\', '/').Replace("//", "/");

            // Check for directory traversal attempts
            if (normalizedPath.Contains("/../") || normalizedPath.Contains("\\..\\") ||
                normalizedPath.StartsWith("../") || normalizedPath.StartsWith("..\\") ||
                normalizedPath.EndsWith("/..") || normalizedPath.EndsWith("\\.."))
            {
                throw new ArgumentException("Path contains directory traversal attempts which are not allowed.", nameof(path));
            }

            // Check for absolute path attempts when base path is set
            if (!string.IsNullOrEmpty(_basePath) && Path.IsPathRooted(normalizedPath))
            {
                throw new ArgumentException("Absolute paths are not allowed when a base path is configured.", nameof(path));
            }

            string fullPath;
            if (string.IsNullOrEmpty(_basePath))
            {
                fullPath = Path.GetFullPath(normalizedPath);
            }
            else
            {
                fullPath = Path.GetFullPath(Path.Combine(_basePath, normalizedPath));

                // Ensure the resolved path is within the base path
                string basePathFull = Path.GetFullPath(_basePath);
                if (!fullPath.StartsWith(basePathFull, StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("Path resolves outside the allowed base directory.", nameof(path));
                }
            }

            return fullPath;
        }

        /// <summary>
        /// Notifies all registered watchers of a file system change.
        /// </summary>
        /// <param name="changeEvent">The change event to notify about.</param>
        private async Task NotifyWatchersAsync(FileSystemChangeEvent changeEvent)
        {
            List<IFileSystemWatcher> watchersToNotify;

            await _watchersSemaphore.WaitAsync();
            try
            {
                watchersToNotify = new List<IFileSystemWatcher>(_watchers);
            }
            finally
            {
                _watchersSemaphore.Release();
            }

            foreach (IFileSystemWatcher watcher in watchersToNotify)
            {
                try
                {
                    await watcher.OnFileSystemChangedAsync(changeEvent);
                }
                catch (Exception)
                {
                    // Ignore exceptions from watchers to prevent them from breaking file operations
                }
            }
        }

        /// <inheritdoc/>
        public Task<Stream> OpenFileForWritingAsync(string path, FileWriteMode mode = FileWriteMode.Overwrite, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            // Ensure directory exists
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Choose file mode based on the write mode parameter
            FileMode fileMode = mode switch
            {
                FileWriteMode.Overwrite => FileMode.Create,
                FileWriteMode.Append => FileMode.Append,
                FileWriteMode.RandomAccess => FileMode.OpenOrCreate,
                _ => throw new ArgumentException($"Unsupported FileWriteMode: {mode}", nameof(mode))
            };

            // Notify watchers of file creation (only for new files, not appends)
            if (mode != FileWriteMode.Append)
            {
                _ = Task.Run(async () => await NotifyWatchersAsync(new FileSystemChangeEvent(FileSystemChangeType.FileAdded, path)));
            }

            return Task.FromResult<Stream>(new FileStream(fullPath, fileMode, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous));
        }

        /// <inheritdoc/>
        public async Task PreAllocateFileAsync(string path, long size, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            // Ensure directory exists
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Pre-allocate the file by creating it with the specified size
            using (FileStream fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous))
            {
                fileStream.SetLength(size);
                await fileStream.FlushAsync(cancellationToken);
            }

            // Notify watchers of file creation
            await NotifyWatchersAsync(new FileSystemChangeEvent(FileSystemChangeType.FileAdded, path));
        }

        /// <inheritdoc/>
        public async Task WriteFileAsTextAsync(string path, string content, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            // Ensure directory exists
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            bool fileExisted = File.Exists(fullPath);
            await File.WriteAllTextAsync(fullPath, content, cancellationToken).ConfigureAwait(false);

            // Notify watchers of file creation (since WriteAllTextAsync creates or overwrites)
            await NotifyWatchersAsync(new FileSystemChangeEvent(FileSystemChangeType.FileAdded, path));
        }

        /// <inheritdoc/>
        public Task<Stream> OpenFileForReadingAsync(string path, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {path}", fullPath);
            }

            return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous));
        }

        /// <inheritdoc/>
        public async Task<string> ReadFileAsTextAsync(string path, Encoding? encoding = null, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {path}", fullPath);
            }

            encoding ??= Encoding.UTF8;
            return await File.ReadAllTextAsync(fullPath, encoding, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                await NotifyWatchersAsync(new FileSystemChangeEvent(FileSystemChangeType.FileDeleted, path));
            }
        }

        /// <inheritdoc/>
        public Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);
            return Task.FromResult(File.Exists(fullPath));
        }

        /// <inheritdoc/>
        public async Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);
            Directory.CreateDirectory(fullPath);
            await NotifyWatchersAsync(new FileSystemChangeEvent(FileSystemChangeType.DirectoryAdded, path));
        }

        /// <inheritdoc/>
        public async Task DeleteDirectoryAsync(string path, bool deleteNonEmpty, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            if (Directory.Exists(fullPath))
            {
                if (deleteNonEmpty)
                {
                    Directory.Delete(fullPath, true);
                }
                else
                {
                    Directory.Delete(fullPath, false);
                }

                await NotifyWatchersAsync(new FileSystemChangeEvent(FileSystemChangeType.DirectoryDeleted, path));
            }
        }

        /// <inheritdoc/>
        public Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);
            return Task.FromResult(Directory.Exists(fullPath));
        }

        /// <inheritdoc/>
        public Task<IEnumerable<string>> EnumerateFilesAsync(string path, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"Directory not found: {path}");
            }

            return Task.FromResult(Directory.EnumerateFiles(fullPath));
        }

        /// <inheritdoc/>
        public Task<long> GetFileSizeAsync(string path, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {path}", fullPath);
            }

            FileInfo fileInfo = new FileInfo(fullPath);
            return Task.FromResult(fileInfo.Length);
        }

        /// <inheritdoc/>
        public Task<DateTime> GetLastModifiedAsync(string path, CancellationToken cancellationToken = default)
        {
            string fullPath = GetFullPath(path);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"File not found: {path}", fullPath);
            }

            FileInfo fileInfo = new FileInfo(fullPath);
            return Task.FromResult(fileInfo.LastWriteTime);
        }

        /// <inheritdoc/>
        public async Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
        {
            string sourceFullPath = GetFullPath(sourcePath);
            string destinationFullPath = GetFullPath(destinationPath);

            if (!File.Exists(sourceFullPath))
            {
                throw new FileNotFoundException($"Source file not found: {sourcePath}", sourceFullPath);
            }

            // Ensure destination directory exists
            string? destinationDirectory = Path.GetDirectoryName(destinationFullPath);
            if (!string.IsNullOrEmpty(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            File.Copy(sourceFullPath, destinationFullPath, true);
            await NotifyWatchersAsync(new FileSystemChangeEvent(FileSystemChangeType.FileAdded, destinationPath));
        }

        /// <inheritdoc/>
        public async Task<IDisposable> AddFileSystemWatcher(IFileSystemWatcher watcher)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
            }

            await _watchersSemaphore.WaitAsync();
            try
            {
                _watchers.Add(watcher);
            }
            finally
            {
                _watchersSemaphore.Release();
            }

            return new WatcherDisposable(this, watcher);
        }

        /// <inheritdoc/>
        public async Task RemoveFileSystemWatcher(IFileSystemWatcher watcher)
        {
            if (watcher == null)
            {
                throw new ArgumentNullException(nameof(watcher));
            }

            await RemoveWatcherAsync(watcher);
        }

        /// <summary>
        /// Removes a watcher from the list of registered watchers.
        /// </summary>
        /// <param name="watcher">The watcher to remove.</param>
        private async Task RemoveWatcherAsync(IFileSystemWatcher watcher)
        {
            await _watchersSemaphore.WaitAsync();
            try
            {
                _watchers.Remove(watcher);
            }
            finally
            {
                _watchersSemaphore.Release();
            }

            // Dispose the watcher if it implements IDisposable
            if (watcher is IDisposable disposable)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception)
                {
                    // Ignore disposal exceptions to prevent them from breaking the removal process
                }
            }
        }
    }
}