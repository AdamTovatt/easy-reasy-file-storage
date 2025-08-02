using EasyReasy.FileStorage;

namespace EasyReasy.FileStorage.Tests;

/// <summary>
/// Test implementation of IFileSystemWatcher for testing file system change notifications.
/// </summary>
public class TestFileSystemWatcher : IFileSystemWatcher
{
    private readonly List<FileSystemChangeEvent> _receivedEvents = new List<FileSystemChangeEvent>();

    /// <summary>
    /// Gets all received change events.
    /// </summary>
    public IReadOnlyList<FileSystemChangeEvent> ReceivedEvents => _receivedEvents.AsReadOnly();

    /// <summary>
    /// Gets the number of received events.
    /// </summary>
    public int EventCount => _receivedEvents.Count;

    /// <summary>
    /// Clears all received events.
    /// </summary>
    public void ClearEvents()
    {
        _receivedEvents.Clear();
    }

    /// <summary>
    /// Gets events of a specific change type.
    /// </summary>
    /// <param name="changeType">The change type to filter by.</param>
    /// <returns>A list of events matching the specified change type.</returns>
    public List<FileSystemChangeEvent> GetEventsByType(FileSystemChangeType changeType)
    {
        return _receivedEvents.Where(e => e.ChangeType == changeType).ToList();
    }

    /// <summary>
    /// Gets events affecting a specific path.
    /// </summary>
    /// <param name="path">The path to filter by.</param>
    /// <returns>A list of events affecting the specified path.</returns>
    public List<FileSystemChangeEvent> GetEventsByPath(string path)
    {
        return _receivedEvents.Where(e => e.AffectedPath == path).ToList();
    }

    /// <summary>
    /// Checks if a specific event was received.
    /// </summary>
    /// <param name="changeType">The expected change type.</param>
    /// <param name="path">The expected affected path.</param>
    /// <returns>True if the event was received, false otherwise.</returns>
    public bool HasReceivedEvent(FileSystemChangeType changeType, string path)
    {
        return _receivedEvents.Any(e => e.ChangeType == changeType && e.AffectedPath == path);
    }

    /// <summary>
    /// Called when a file system change occurs.
    /// </summary>
    /// <param name="change">The change event that occurred.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public Task OnFileSystemChangedAsync(FileSystemChangeEvent change)
    {
        _receivedEvents.Add(change);
        return Task.CompletedTask;
    }
} 