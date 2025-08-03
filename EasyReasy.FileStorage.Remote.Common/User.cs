namespace EasyReasy.FileStorage.Remote.Common
{
    /// <summary>
    /// Represents a user in the file storage system.
    /// </summary>
    /// <param name="Id">The unique user identifier (username).</param>
    /// <param name="PasswordHash">The hashed password.</param>
    /// <param name="IsAdmin">Whether the user has admin privileges.</param>
    /// <param name="StorageLimitBytes">The storage limit in bytes for this user.</param>
    public record User(string Id, string PasswordHash, bool IsAdmin = false, long StorageLimitBytes = 1024 * 1024 * 1024); // Default 1GB
}
