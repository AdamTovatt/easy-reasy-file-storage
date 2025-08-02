using System.Text;

namespace EasyReasy.FileStorage;

/// <summary>
/// Defines operations for file system access with asynchronous methods.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Opens a file for writing and returns a stream for incremental writing.
    /// This method is optimized for large files and provides low memory footprint through streaming.
    /// </summary>
    /// <param name="path">The path where the file should be written.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous open operation, containing a stream for writing the file content.</returns>
    Task<Stream> OpenFileForWritingAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes text content to a file at the specified path.
    /// </summary>
    /// <param name="path">The path where the file should be written.</param>
    /// <param name="content">The text content to write.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    Task WriteFileAsTextAsync(string path, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a file for reading and returns a stream for incremental access.
    /// This method is optimized for large files and provides low memory footprint through streaming.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous open operation, containing a stream for reading the file content.</returns>
    Task<Stream> OpenFileForReadingAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads a file and returns its content as text.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <param name="encoding">The encoding to use when reading the file. If null, uses UTF-8.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous read operation, containing the file content as text.</returns>
    Task<string> ReadFileAsTextAsync(string path, Encoding? encoding = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to delete.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists at the specified path.
    /// </summary>
    /// <param name="path">The path of the file to check.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous check operation, containing true if the file exists, false otherwise.</returns>
    Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a directory at the specified path.
    /// </summary>
    /// <param name="path">The path where the directory should be created.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous create operation.</returns>
    Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a directory at the specified path.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    /// <param name="deleteNonEmpty">Whether to delete the directory even if it contains files or subdirectories.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteDirectoryAsync(string path, bool deleteNonEmpty, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a directory exists at the specified path.
    /// </summary>
    /// <param name="path">The path of the directory to check.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous check operation, containing true if the directory exists, false otherwise.</returns>
    Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates all files in the specified directory.
    /// </summary>
    /// <param name="path">The path of the directory to enumerate.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous enumeration operation, containing the collection of file paths.</returns>
    Task<IEnumerable<string>> EnumerateFilesAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    /// <param name="path">The path of the file to get the size for.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous size retrieval operation, containing the file size in bytes.</returns>
    Task<long> GetFileSizeAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the last modified date and time of a file.
    /// </summary>
    /// <param name="path">The path of the file to get the last modified time for.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous time retrieval operation, containing the last modified date and time.</returns>
    Task<DateTime> GetLastModifiedAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file from the source path to the destination path.
    /// </summary>
    /// <param name="sourcePath">The path of the source file to copy.</param>
    /// <param name="destinationPath">The path where the file should be copied to.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
}
