using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyReasy.FileStorage.Tests;

[TestClass]
public class LocalFileSystemWatcherTests
{
    private string _testBasePath = null!;
    private LocalFileSystem _fileSystem = null!;
    private TestFileSystemWatcher _testWatcher = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _testBasePath = Path.Combine(Path.GetTempPath(), $"EasyReasyWatcherTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testBasePath);
        _fileSystem = new LocalFileSystem(_testBasePath);
        _testWatcher = new TestFileSystemWatcher();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testBasePath))
        {
            Directory.Delete(_testBasePath, true);
        }
    }

    [TestMethod]
    public async Task AddFileSystemWatcher_ShouldAddWatcher()
    {
        // Act
        IDisposable disposable = await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Assert
        Assert.IsNotNull(disposable);
    }

    [TestMethod]
    public async Task RemoveFileSystemWatcher_ShouldRemoveWatcher()
    {
        // Arrange
        IDisposable disposable = await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act
        await _fileSystem.RemoveFileSystemWatcher(_testWatcher);

        // Assert - Create a file and verify no notification is received
        await _fileSystem.WriteFileAsTextAsync("test.txt", "content");
        Assert.AreEqual(0, _testWatcher.EventCount);
    }

    [TestMethod]
    public async Task WriteFileAsTextAsync_ShouldNotifyFileAdded()
    {
        // Arrange
        await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act
        await _fileSystem.WriteFileAsTextAsync("test.txt", "content");

        // Assert
        Assert.AreEqual(1, _testWatcher.EventCount);
        Assert.IsTrue(_testWatcher.HasReceivedEvent(FileSystemChangeType.FileAdded, "test.txt"));
    }

    [TestMethod]
    public async Task DeleteFileAsync_ShouldNotifyFileDeleted()
    {
        // Arrange
        await _fileSystem.WriteFileAsTextAsync("test.txt", "content");
        await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act
        await _fileSystem.DeleteFileAsync("test.txt");

        // Assert
        Assert.AreEqual(1, _testWatcher.EventCount);
        Assert.IsTrue(_testWatcher.HasReceivedEvent(FileSystemChangeType.FileDeleted, "test.txt"));
    }

    [TestMethod]
    public async Task CreateDirectoryAsync_ShouldNotifyDirectoryAdded()
    {
        // Arrange
        await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act
        await _fileSystem.CreateDirectoryAsync("new-directory");

        // Assert
        Assert.AreEqual(1, _testWatcher.EventCount);
        Assert.IsTrue(_testWatcher.HasReceivedEvent(FileSystemChangeType.DirectoryAdded, "new-directory"));
    }

    [TestMethod]
    public async Task DeleteDirectoryAsync_ShouldNotifyDirectoryDeleted()
    {
        // Arrange
        await _fileSystem.CreateDirectoryAsync("test-directory");
        await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act
        await _fileSystem.DeleteDirectoryAsync("test-directory", true);

        // Assert
        Assert.AreEqual(1, _testWatcher.EventCount);
        Assert.IsTrue(_testWatcher.HasReceivedEvent(FileSystemChangeType.DirectoryDeleted, "test-directory"));
    }

    [TestMethod]
    public async Task CopyFileAsync_ShouldNotifyFileAdded()
    {
        // Arrange
        await _fileSystem.WriteFileAsTextAsync("source.txt", "content");
        await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act
        await _fileSystem.CopyFileAsync("source.txt", "destination.txt");

        // Assert
        Assert.AreEqual(1, _testWatcher.EventCount);
        Assert.IsTrue(_testWatcher.HasReceivedEvent(FileSystemChangeType.FileAdded, "destination.txt"));
    }

    [TestMethod]
    public async Task MultipleWatchers_ShouldNotifyAllWatchers()
    {
        // Arrange
        TestFileSystemWatcher secondWatcher = new TestFileSystemWatcher();
        await _fileSystem.AddFileSystemWatcher(_testWatcher);
        await _fileSystem.AddFileSystemWatcher(secondWatcher);

        // Act
        await _fileSystem.WriteFileAsTextAsync("test.txt", "content");

        // Assert
        Assert.AreEqual(1, _testWatcher.EventCount);
        Assert.AreEqual(1, secondWatcher.EventCount);
        Assert.IsTrue(_testWatcher.HasReceivedEvent(FileSystemChangeType.FileAdded, "test.txt"));
        Assert.IsTrue(secondWatcher.HasReceivedEvent(FileSystemChangeType.FileAdded, "test.txt"));
    }

    [TestMethod]
    public async Task DisposableWatcher_ShouldRemoveWatcherOnDispose()
    {
        // Arrange
        IDisposable disposable = await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act
        disposable.Dispose();

        // Give the async removal operation time to complete
        await Task.Delay(10);

        // Assert - Create a file and verify no notification is received
        await _fileSystem.WriteFileAsTextAsync("test.txt", "content");
        Assert.AreEqual(0, _testWatcher.EventCount);
    }

    [TestMethod]
    public async Task MultipleOperations_ShouldNotifyCorrectEvents()
    {
        // Arrange
        await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act
        await _fileSystem.WriteFileAsTextAsync("file1.txt", "content1");
        await _fileSystem.CreateDirectoryAsync("directory1");
        await _fileSystem.WriteFileAsTextAsync("file2.txt", "content2");
        await _fileSystem.DeleteFileAsync("file1.txt");

        // Assert
        Assert.AreEqual(4, _testWatcher.EventCount);
        
        List<FileSystemChangeEvent> fileAddedEvents = _testWatcher.GetEventsByType(FileSystemChangeType.FileAdded);
        Assert.AreEqual(2, fileAddedEvents.Count);
        Assert.IsTrue(fileAddedEvents.Any(e => e.AffectedPath == "file1.txt"));
        Assert.IsTrue(fileAddedEvents.Any(e => e.AffectedPath == "file2.txt"));

        List<FileSystemChangeEvent> directoryAddedEvents = _testWatcher.GetEventsByType(FileSystemChangeType.DirectoryAdded);
        Assert.AreEqual(1, directoryAddedEvents.Count);
        Assert.IsTrue(directoryAddedEvents.Any(e => e.AffectedPath == "directory1"));

        List<FileSystemChangeEvent> fileDeletedEvents = _testWatcher.GetEventsByType(FileSystemChangeType.FileDeleted);
        Assert.AreEqual(1, fileDeletedEvents.Count);
        Assert.IsTrue(fileDeletedEvents.Any(e => e.AffectedPath == "file1.txt"));
    }

    [TestMethod]
    public async Task WatcherExceptions_ShouldNotBreakFileOperations()
    {
        // Arrange
        ExceptionThrowingWatcher exceptionWatcher = new ExceptionThrowingWatcher();
        await _fileSystem.AddFileSystemWatcher(exceptionWatcher);
        await _fileSystem.AddFileSystemWatcher(_testWatcher);

        // Act & Assert - Should not throw and other watchers should still receive notifications
        await _fileSystem.WriteFileAsTextAsync("test.txt", "content");
        Assert.AreEqual(1, _testWatcher.EventCount);
        Assert.IsTrue(_testWatcher.HasReceivedEvent(FileSystemChangeType.FileAdded, "test.txt"));
    }

    [TestMethod]
    public async Task RemoveNonExistentWatcher_ShouldNotThrow()
    {
        // Arrange
        TestFileSystemWatcher nonExistentWatcher = new TestFileSystemWatcher();

        // Act & Assert - Should not throw
        await _fileSystem.RemoveFileSystemWatcher(nonExistentWatcher);
    }

    [TestMethod]
    public async Task AddNullWatcher_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
        {
            await _fileSystem.AddFileSystemWatcher(null!);
        });
    }

    [TestMethod]
    public async Task RemoveNullWatcher_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
        {
            await _fileSystem.RemoveFileSystemWatcher(null!);
        });
    }

    /// <summary>
    /// Test watcher that throws exceptions to test error handling.
    /// </summary>
    private class ExceptionThrowingWatcher : IFileSystemWatcher
    {
        public Task OnFileSystemChangedAsync(FileSystemChangeEvent change)
        {
            throw new InvalidOperationException("Test exception from watcher");
        }
    }

    /// <summary>
    /// Test watcher that implements IDisposable to test disposal behavior.
    /// </summary>
    private class DisposableTestWatcher : IFileSystemWatcher, IDisposable
    {
        public bool IsDisposed { get; private set; }
        public int DisposeCallCount { get; private set; }

        public Task OnFileSystemChangedAsync(FileSystemChangeEvent change)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            IsDisposed = true;
            DisposeCallCount++;
        }
    }

    [TestMethod]
    public async Task RemoveFileSystemWatcher_ShouldDisposeWatcherIfItImplementsIDisposable()
    {
        // Arrange
        DisposableTestWatcher disposableWatcher = new DisposableTestWatcher();
        await _fileSystem.AddFileSystemWatcher(disposableWatcher);

        // Act
        await _fileSystem.RemoveFileSystemWatcher(disposableWatcher);

        // Assert
        Assert.IsTrue(disposableWatcher.IsDisposed);
        Assert.AreEqual(1, disposableWatcher.DisposeCallCount);
    }

    [TestMethod]
    public async Task DisposableWatcher_ShouldDisposeWatcherOnDispose()
    {
        // Arrange
        DisposableTestWatcher disposableWatcher = new DisposableTestWatcher();
        IDisposable disposable = await _fileSystem.AddFileSystemWatcher(disposableWatcher);

        // Act
        disposable.Dispose();

        // Give the async removal operation time to complete
        await Task.Delay(10);

        // Assert
        Assert.IsTrue(disposableWatcher.IsDisposed);
        Assert.AreEqual(1, disposableWatcher.DisposeCallCount);
    }
} 