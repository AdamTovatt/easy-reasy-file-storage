using System.CommandLine;

namespace EasyReasy.FileStorage.Server.Commands
{
    /// <summary>
    /// Main CLI application that handles command routing and execution.
    /// </summary>
    public class CliApplication
    {
        private readonly RootCommand _rootCommand;

        public CliApplication()
        {
            _rootCommand = new RootCommand("EasyReasy File Storage Server CLI")
            {
                new CreateTenantCommand(),
                new CreateUserCommand(),
            };
        }

        /// <summary>
        /// Executes the CLI application with the provided arguments.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        /// <returns>Exit code (0 for success, non-zero for failure).</returns>
        public async Task<int> ExecuteAsync(string[] args)
        {
            return await _rootCommand.InvokeAsync(args);
        }
    }
}