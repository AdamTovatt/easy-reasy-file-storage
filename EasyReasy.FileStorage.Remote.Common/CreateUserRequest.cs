namespace EasyReasy.FileStorage.Remote.Common
{
    /// <summary>
    /// Request model for creating a new user.
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// The username for the new user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password for the new user.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Whether the user should have admin privileges.
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// The storage limit in bytes for this user.
        /// </summary>
        public long StorageLimitBytes { get; set; }

        public CreateUserRequest(string username, string password, bool isAdmin = false, long storageLimitBytes = 1024 * 1024 * 1024)
        {
            Username = username;
            Password = password;
            IsAdmin = isAdmin;
            StorageLimitBytes = storageLimitBytes;
        }
    }
}
