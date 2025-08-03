using EasyReasy.EnvironmentVariables;

namespace EasyReasy.FileStorage.Server
{
    [EnvironmentVariableNameContainer]
    public static class EnvironmentVariable
    {
        /// <summary>
        /// JWT signing secret for authentication tokens.
        /// Must be at least 32 bytes (256 bits) for HS256 security.
        /// </summary>
        [EnvironmentVariableName(minLength: 32)]
        public static readonly VariableName JwtSigningSecret = new VariableName("JWT_SIGNING_SECRET");

        /// <summary>
        /// Base path for file storage where tenant and user folders are located.
        /// Structure: BaseStoragePath/tenant/user/.password and BaseStoragePath/tenant/user/files/
        /// </summary>
        [EnvironmentVariableName]
        public static readonly VariableName BaseStoragePath = new VariableName("BASE_STORAGE_PATH");
    }
}