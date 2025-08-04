using EasyReasy.Auth;

namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// Factory implementation for creating tenant-specific FileSystemUserService instances.
    /// </summary>
    public class FileSystemUserServiceFactory : IUserServiceFactory
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<FileSystemUserServiceFactory> _logger;
        private readonly IBasePathProvider _basePathProvider;

        public FileSystemUserServiceFactory(IPasswordHasher passwordHasher, ILogger<FileSystemUserServiceFactory> logger, IBasePathProvider basePathProvider)
        {
            _passwordHasher = passwordHasher;
            _logger = logger;
            _basePathProvider = basePathProvider;

            string absoluteBasePath = _basePathProvider.GetRootPath();
            _logger.LogInformation("FileSystemUserServiceFactory initialized with base path: {AbsoluteBasePath}", absoluteBasePath);
        }

        /// <summary>
        /// Creates a FileSystemUserService instance for the specified tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A FileSystemUserService instance scoped to the specified tenant.</returns>
        public IUserService CreateUserService(string tenantId)
        {
            string dataFolderPath = _basePathProvider.GetDataPath();
            return new FileSystemUserService(_passwordHasher, tenantId, dataFolderPath);
        }
    }
}