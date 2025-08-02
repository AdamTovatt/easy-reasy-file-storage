namespace EasyReasy.FileStorage;

/// <summary>
/// Represents a change event that occurred in the file system.
/// </summary>
/// <param name="ChangeType">The type of change that occurred.</param>
/// <param name="AffectedPath">The path that was affected by the change.</param>
public record FileSystemChangeEvent(FileSystemChangeType ChangeType, string AffectedPath);