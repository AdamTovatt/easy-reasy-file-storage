using EasyReasy.Auth;
using EasyReasy.EnvironmentVariables;
using EasyReasy.FileStorage.Remote.Common;

namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// File system-based implementation of IUserService.
    /// Reads user data from a hierarchical file system structure:
    /// BaseStoragePath/tenantId/userId/.password and BaseStoragePath/tenantId/userId/files/
    /// </summary>
    public class FileSystemUserService : IUserService
    {
        private readonly string _baseStoragePath;
        private readonly string _tenantId;
        private readonly IPasswordHasher _passwordHasher;

        public FileSystemUserService(IPasswordHasher passwordHasher, string tenantId)
        {
            _passwordHasher = passwordHasher;
            _tenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
            _baseStoragePath = EnvironmentVariable.BaseStoragePath.GetValue();

            // Ensure the base storage path exists
            if (!Directory.Exists(_baseStoragePath))
            {
                Directory.CreateDirectory(_baseStoragePath);
            }

            // Ensure the tenant directory exists
            string tenantDirectoryPath = Path.Combine(_baseStoragePath, _tenantId);
            if (!Directory.Exists(tenantDirectoryPath))
            {
                Directory.CreateDirectory(tenantDirectoryPath);
            }
        }

        public async Task<User?> GetUserByNameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            string userDirectoryPath = Path.Combine(_baseStoragePath, _tenantId, username);
            string passwordFilePath = Path.Combine(userDirectoryPath, ".password");

            // Check if the user directory exists
            if (!Directory.Exists(userDirectoryPath))
            {
                return null;
            }

            // Check if the password file exists
            if (!File.Exists(passwordFilePath))
            {
                return null;
            }

            try
            {
                // Read the password hash from the .password file
                string passwordHash = await File.ReadAllTextAsync(passwordFilePath);
                passwordHash = passwordHash.Trim();

                if (string.IsNullOrEmpty(passwordHash))
                {
                    return null;
                }

                return new User(username, passwordHash);
            }
            catch (Exception)
            {
                // If we can't read the password file, the user is invalid
                return null;
            }
        }

        /// <summary>
        /// Creates a new user in the file system within the current tenant.
        /// This is an admin-only operation.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The plain text password.</param>
        /// <returns>True if the user was created successfully, false otherwise.</returns>
        public async Task<bool> CreateUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            string userDirectoryPath = Path.Combine(_baseStoragePath, _tenantId, username);
            string passwordFilePath = Path.Combine(userDirectoryPath, ".password");
            string filesDirectoryPath = Path.Combine(userDirectoryPath, "files");

            try
            {
                // Create the user directory
                Directory.CreateDirectory(userDirectoryPath);

                // Create the files directory
                Directory.CreateDirectory(filesDirectoryPath);

                // Create the password file with the hashed password
                string passwordHash = _passwordHasher.HashPassword(password, username);
                await File.WriteAllTextAsync(passwordFilePath, passwordHash);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}