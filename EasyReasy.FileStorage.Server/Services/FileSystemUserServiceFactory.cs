using EasyReasy.Auth;

namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// Factory implementation for creating tenant-specific FileSystemUserService instances.
    /// </summary>
    public class FileSystemUserServiceFactory : IUserServiceFactory
    {
        private readonly IPasswordHasher _passwordHasher;

        public FileSystemUserServiceFactory(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Creates a FileSystemUserService instance for the specified tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A FileSystemUserService instance scoped to the specified tenant.</returns>
        public IUserService CreateUserService(string tenantId)
        {
            return new FileSystemUserService(_passwordHasher, tenantId);
        }
    }
}