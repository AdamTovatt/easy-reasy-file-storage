using EasyReasy.Auth;
using EasyReasy.EnvironmentVariables;
using EasyReasy.FileStorage.Remote.Common;
using EasyReasy.FileStorage.Server.Configuration;
using System.Text.Json;

namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// File system-based implementation of IUserService.
    /// Reads user data from a hierarchical file system structure:
    /// BaseStoragePath/tenantId/userId/user.json and BaseStoragePath/tenantId/userId/files/
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
            string userJsonFilePath = Path.Combine(userDirectoryPath, "user.json");

            // Check if the user directory exists
            if (!Directory.Exists(userDirectoryPath))
            {
                return null;
            }

            // Check if the user.json file exists
            if (!File.Exists(userJsonFilePath))
            {
                return null;
            }

            try
            {
                // Read and deserialize the user data from the user.json file
                string userJson = await File.ReadAllTextAsync(userJsonFilePath);
                User? user = JsonSerializer.Deserialize<User>(userJson, JsonConfiguration.DefaultOptions);

                if (user == null)
                {
                    return null;
                }

                return user;
            }
            catch (Exception)
            {
                // If we can't read or deserialize the user file, the user is invalid
                return null;
            }
        }

        /// <summary>
        /// Creates a new user in the file system within the current tenant.
        /// This is an admin-only operation.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The plain text password.</param>
        /// <param name="isAdmin">Whether the user should have admin privileges.</param>
        /// <param name="storageLimitBytes">The storage limit in bytes for this user.</param>
        /// <returns>True if the user was created successfully, false otherwise.</returns>
        public async Task<bool> CreateUserAsync(string username, string password, bool isAdmin = false, long storageLimitBytes = 1024 * 1024 * 1024)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            string userDirectoryPath = Path.Combine(_baseStoragePath, _tenantId, username);
            string userJsonFilePath = Path.Combine(userDirectoryPath, "user.json");
            string filesDirectoryPath = Path.Combine(userDirectoryPath, "files");

            try
            {
                // Create the user directory
                Directory.CreateDirectory(userDirectoryPath);

                // Create the files directory
                Directory.CreateDirectory(filesDirectoryPath);

                // Create the user data with hashed password
                string passwordHash = _passwordHasher.HashPassword(password, username);
                User user = new User(username, passwordHash, isAdmin, storageLimitBytes);

                // Serialize and save the user data to user.json
                string userJson = JsonSerializer.Serialize(user, JsonConfiguration.DefaultOptions);
                await File.WriteAllTextAsync(userJsonFilePath, userJson);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}