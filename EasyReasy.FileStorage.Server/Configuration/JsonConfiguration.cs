using System.Text.Json;

namespace EasyReasy.FileStorage.Server.Configuration
{
    /// <summary>
    /// Provides default JSON serialization options for the application.
    /// </summary>
    public static class JsonConfiguration
    {
        /// <summary>
        /// Default JSON serialization options with camelCase property naming.
        /// </summary>
        public static JsonSerializerOptions DefaultOptions { get; } = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }
} 