namespace EasyReasy.FileStorage;

/// <summary>
/// Defines the types of changes that can occur in the file system.
/// </summary>
public enum FileSystemChangeType
{
    /// <summary>
    /// A file was added to the file system.
    /// </summary>
    FileAdded,

    /// <summary>
    /// A file was deleted from the file system.
    /// </summary>
    FileDeleted,

    /// <summary>
    /// A directory was added to the file system.
    /// </summary>
    DirectoryAdded,

    /// <summary>
    /// A directory was deleted from the file system.
    /// </summary>
    DirectoryDeleted
}