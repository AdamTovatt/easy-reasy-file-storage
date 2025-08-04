using EasyReasy.EnvironmentVariables;
using Microsoft.Extensions.Logging;

namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// Implementation of IBasePathProvider that resolves paths from environment variables.
    /// </summary>
    public class BasePathProvider : IBasePathProvider
    {
        private readonly ILogger<BasePathProvider> _logger;

        public BasePathProvider(ILogger<BasePathProvider> logger)
        {
            _logger = logger;
            EnsureFoldersExist();
        }

        public string GetRootPath()
        {
            string baseStoragePath = EnvironmentVariable.BaseStoragePath.GetValue();
            return Path.GetFullPath(baseStoragePath);
        }

        public string GetDataPath()
        {
            string rootPath = GetRootPath();
            return Path.Combine(rootPath, "data");
        }

        private void EnsureFoldersExist()
        {
            string rootPath = GetRootPath();
            string dataFolderPath = GetDataPath();

            if (!Directory.Exists(rootPath))
            {
                try
                {
                    Directory.CreateDirectory(rootPath);
                    _logger.LogInformation("Base storage folder created at: {BaseStoragePath}", rootPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating base storage folder at {BaseStoragePath}", rootPath);
                    throw; // Re-throw to let the DI container handle the error
                }
            }

            if (!Directory.Exists(dataFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(dataFolderPath);
                    _logger.LogInformation("Data folder created at: {DataFolderPath}", dataFolderPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating data folder at {DataFolderPath}", dataFolderPath);
                    throw; // Re-throw to let the DI container handle the error
                }
            }
        }
    }
} 