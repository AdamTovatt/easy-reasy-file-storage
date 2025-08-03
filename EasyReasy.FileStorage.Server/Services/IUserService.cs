using EasyReasy.FileStorage.Remote.Common;

namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// Service for managing user authentication and retrieval.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <param name="username">The username to search for.</param>
        /// <returns>The user if found, null otherwise.</returns>
        Task<User?> GetUserByNameAsync(string username);

        /// <summary>
        /// Creates a new user in the current tenant.
        /// This is an admin-only operation.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The plain text password.</param>
        /// <returns>True if the user was created successfully, false otherwise.</returns>
        Task<bool> CreateUserAsync(string username, string password);
    }
}