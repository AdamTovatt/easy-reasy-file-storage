using EasyReasy.EnvironmentVariables;
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
            // Check for help flags
            if (IsHelpRequest(args))
            {
                ShowCliHelp();
                return 0;
            }

            // Load environment variables from systemd service file in CLI mode
            bool environmentLoaded = LoadEnvironmentFromSystemdServiceFileAsync();
            if (!environmentLoaded)
            {
                return 1; // Exit with error code if environment loading failed
            }

            // Validate environment variables after loading
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariable));

            return await _rootCommand.InvokeAsync(args);
        }

        private static bool IsHelpRequest(string[] args)
        {
            string[] helpFlags = { "-h", "--help", "-help", "/h", "/help" };
            return args.Any(arg => helpFlags.Contains(arg.ToLowerInvariant()));
        }

        private static void ShowCliHelp()
        {
            Console.WriteLine("EasyReasy File Storage Server CLI");
            Console.WriteLine("=================================");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run [command] [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  create-tenant <tenant-id>  Create a new tenant");
            Console.WriteLine("  create-user <tenant-id> <username> <password>  Create a new user in a tenant");
            Console.WriteLine();
            Console.WriteLine("Environment Variables:");
            Console.WriteLine("  When running in CLI mode, you will be prompted to provide the path to a");
            Console.WriteLine("  systemd service file that contains the required environment variables:");
            Console.WriteLine();
            Console.WriteLine("  - JWT_SIGNING_SECRET: Secret for JWT token signing (min 32 characters)");
            Console.WriteLine("  - BASE_STORAGE_PATH: Base path for file storage");
            Console.WriteLine();
            Console.WriteLine("Example systemd service file format:");
            Console.WriteLine("  [Service]");
            Console.WriteLine("  Environment=\"JWT_SIGNING_SECRET=your-secret-here-min-32-chars\"");
            Console.WriteLine("  Environment=\"BASE_STORAGE_PATH=/var/lib/easyreasy/storage\"");
            Console.WriteLine();
            Console.WriteLine("Help:");
            Console.WriteLine("  -h, --help, -help, /h, /help  Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run create-tenant my-tenant");
            Console.WriteLine("  dotnet run create-user my-tenant john mypassword");
            Console.WriteLine();
        }

        private static bool LoadEnvironmentFromSystemdServiceFileAsync()
        {
            Console.WriteLine("CLI mode detected. Environment variables need to be loaded from a systemd service file.");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Please enter the path to the systemd service file: ");
                string? filePath = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    Console.WriteLine("Error: File path cannot be empty. Please try again.");
                    Console.WriteLine();
                    continue;
                }

                if (!File.Exists(filePath))
                {
                    string absolutePath = Path.GetFullPath(filePath);
                    Console.WriteLine($"Error: File not found at '{absolutePath}'. Please check the path and try again.");
                    Console.WriteLine();
                    continue;
                }

                try
                {
                    // Load environment variables from the systemd service file
                    EnvironmentVariableHelper.LoadVariablesFromFile(filePath, new SystemdServiceFilePreprocessor());
                    Console.WriteLine($"Successfully loaded environment variables from '{filePath}'.");
                    Console.WriteLine();
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading environment variables from '{filePath}': {ex.Message}");
                    Console.WriteLine("Please check the file format and try again.");
                    Console.WriteLine();
                }
            }
        }
    }
}