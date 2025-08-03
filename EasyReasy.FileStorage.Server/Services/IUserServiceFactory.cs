using EasyReasy.FileStorage.Remote.Common;

namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// Factory for creating tenant-specific user service instances.
    /// </summary>
    public interface IUserServiceFactory
    {
        /// <summary>
        /// Creates a user service instance for the specified tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A user service instance scoped to the specified tenant.</returns>
        IUserService CreateUserService(string tenantId);
    }
} 