namespace EasyReasy.FileStorage.Server.Commands
{
    /// <summary>
    /// Interface for CLI command handlers.
    /// </summary>
    public interface ICliCommandHandler
    {
        /// <summary>
        /// Executes the command with the provided arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failure).</returns>
        Task<int> ExecuteAsync(string[] args);
    }
}