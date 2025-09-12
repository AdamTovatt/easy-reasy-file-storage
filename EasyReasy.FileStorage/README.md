# EasyReasy.FileStorage

[← Back to Repository Overview](../README.md)

[![NuGet](https://img.shields.io/badge/nuget-EasyReasy.FileStorage-blue.svg)](https://www.nuget.org/packages/EasyReasy.FileStorage)

A .NET library providing abstracted file system operations with support for file watching and security features.

## Overview

EasyReasy.FileStorage provides a clean abstraction over file system operations with built-in security measures to prevent directory traversal attacks. It supports both basic file operations and file system watching capabilities.

**Key Features:**
- **Security**: Built-in protection against directory traversal attacks
- **Async Operations**: All file operations are asynchronous
- **File Watching**: Support for monitoring file system changes
- **Base Path Isolation**: Operations can be scoped to a specific base directory
- **Streaming Support**: Optimized for large files with streaming operations

## Quick Start

```csharp
// Create a file system instance with a base path
IFileSystem fileSystem = new LocalFileSystem("/path/to/base/directory");

// Write a file
await fileSystem.WriteFileAsTextAsync("example.txt", "Hello, World!");

// Read a file
string content = await fileSystem.ReadFileAsTextAsync("example.txt");

// Check if file exists
bool exists = await fileSystem.FileExistsAsync("example.txt");
```

## Core Concepts

### IFileSystem
The main interface for file system operations:

```csharp
public interface IFileSystem
{
    Task<Stream> OpenFileForWritingAsync(string path, FileWriteMode mode = FileWriteMode.Overwrite, CancellationToken cancellationToken = default);
    Task PreAllocateFileAsync(string path, long size, CancellationToken cancellationToken = default);
    Task WriteFileAsTextAsync(string path, string content, CancellationToken cancellationToken = default);
    Task<Stream> OpenFileForReadingAsync(string path, CancellationToken cancellationToken = default);
    Task<string> ReadFileAsTextAsync(string path, Encoding? encoding = null, CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string path, CancellationToken cancellationToken = default);
    Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default);
    Task CreateDirectoryAsync(string path, CancellationToken cancellationToken = default);
    Task DeleteDirectoryAsync(string path, bool deleteNonEmpty, CancellationToken cancellationToken = default);
    Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> EnumerateFilesAsync(string path, CancellationToken cancellationToken = default);
    Task<long> GetFileSizeAsync(string path, CancellationToken cancellationToken = default);
    Task<DateTime> GetLastModifiedAsync(string path, CancellationToken cancellationToken = default);
    Task CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
}
```

### IWatchableFileSystem
Interface for file systems that support watching for changes:

```csharp
public interface IWatchableFileSystem
{
    Task<IDisposable> AddFileSystemWatcher(IFileSystemWatcher watcher);
    Task RemoveFileSystemWatcher(IFileSystemWatcher watcher);
}
```

### IFileSystemWatcher
Interface for components that want to be notified of file system changes:

```csharp
public interface IFileSystemWatcher
{
    Task OnFileSystemChangedAsync(FileSystemChangeEvent change);
}
```

### LocalFileSystem
The primary implementation that provides local file system operations:

```csharp
public class LocalFileSystem : IFileSystem, IWatchableFileSystem
{
    public LocalFileSystem(string basePath);
}
```

## Getting Started

### 1. Create a File System Instance

```csharp
// With base path (all operations will be relative to this path)
LocalFileSystem fileSystem = new LocalFileSystem("/path/to/base/directory");

// Without base path (allows absolute paths)
LocalFileSystem fileSystem = new LocalFileSystem(string.Empty);
```

### 2. Basic File Operations

```csharp
// Write text to a file
await fileSystem.WriteFileAsTextAsync("documents/example.txt", "File content here");

// Read text from a file
string content = await fileSystem.ReadFileAsTextAsync("documents/example.txt");

// Check if file exists
bool exists = await fileSystem.FileExistsAsync("documents/example.txt");

// Delete a file
await fileSystem.DeleteFileAsync("documents/example.txt");
```

### 3. Directory Operations

```csharp
// Create a directory
await fileSystem.CreateDirectoryAsync("documents/subfolder");

// Check if directory exists
bool dirExists = await fileSystem.DirectoryExistsAsync("documents/subfolder");

// List files in directory
IEnumerable<string> files = await fileSystem.EnumerateFilesAsync("documents");
```

### 4. File Watching

```csharp
// Create a watcher
IFileSystemWatcher watcher = new MyFileWatcher();

// Add watcher to file system
IDisposable disposable = await fileSystem.AddFileSystemWatcher(watcher);

// Remove watcher when done
disposable.Dispose();
```

### 5. Streaming Operations

```csharp
// Write large files efficiently (overwrites existing file)
using (Stream writeStream = await fileSystem.OpenFileForWritingAsync("large-file.dat", FileWriteMode.Overwrite))
{
    byte[] buffer = new byte[8192];
    // Write data to stream
}

// Append to existing files
using (Stream writeStream = await fileSystem.OpenFileForWritingAsync("log-file.txt", FileWriteMode.Append))
{
    byte[] logEntry = Encoding.UTF8.GetBytes("New log entry\n");
    await writeStream.WriteAsync(logEntry, 0, logEntry.Length);
}

// Random access writing (for chunked uploads, database files, etc.)
await fileSystem.PreAllocateFileAsync("chunked-file.dat", 1024 * 1024); // Pre-allocate 1MB
using (Stream writeStream = await fileSystem.OpenFileForWritingAsync("chunked-file.dat", FileWriteMode.RandomAccess))
{
    // Write chunk at specific position
    writeStream.Seek(512 * 1024, SeekOrigin.Begin); // Seek to 512KB position
    byte[] chunk = new byte[1024];
    await writeStream.WriteAsync(chunk, 0, chunk.Length);
}

// Read large files efficiently
using (Stream readStream = await fileSystem.OpenFileForReadingAsync("large-file.dat"))
{
    byte[] buffer = new byte[8192];
    // Read data from stream
}
```

### FileWriteMode

The `FileWriteMode` enum controls how files are opened for writing:

```csharp
public enum FileWriteMode
{
    Overwrite,     // Truncates existing file or creates new file (default)
    Append,        // Appends to end of existing file
    RandomAccess   // Preserves existing content, allows seeking to any position
}
```

## File System Change Events

The library supports monitoring file system changes through events:

```csharp
public record FileSystemChangeEvent(FileSystemChangeType ChangeType, string AffectedPath);

public enum FileSystemChangeType
{
    FileAdded,
    FileDeleted,
    DirectoryAdded,
    DirectoryDeleted
}
```

## Security Features

The `LocalFileSystem` implementation includes security measures:

- **Directory Traversal Protection**: Prevents `../` and `..\` patterns
- **Base Path Isolation**: Ensures operations stay within the configured base directory
- **Path Validation**: Validates paths for invalid characters and patterns

## Dependencies

- **.NET 8.0+**: Modern .NET features and performance optimizations

## License
MIT
