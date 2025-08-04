namespace EasyReasy.FileStorage.Server.Services
{
    /// <summary>
    /// Provides access to base storage paths with consistent path resolution.
    /// </summary>
    public interface IBasePathProvider
    {
        /// <summary>
        /// Gets the full absolute path to the root storage folder.
        /// </summary>
        /// <returns>The absolute path to the root storage folder.</returns>
        string GetRootPath();

        /// <summary>
        /// Gets the full absolute path to the data folder.
        /// </summary>
        /// <returns>The absolute path to the data folder.</returns>
        string GetDataPath();
    }
} 