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
            Console.WriteLine("  create-tenant    Creates a new tenant directory structure");
            Console.WriteLine("  create-user      Creates a new user within a specified tenant");
            Console.WriteLine();
            Console.WriteLine("Command Details:");
            Console.WriteLine();
            Console.WriteLine("create-tenant:");
            Console.WriteLine("  Description:");
            Console.WriteLine("    Creates a new tenant directory structure");
            Console.WriteLine();
            Console.WriteLine("  Usage:");
            Console.WriteLine("    dotnet run create-tenant [options]");
            Console.WriteLine();
            Console.WriteLine("  Options:");
            Console.WriteLine("    -n, --name <name> (REQUIRED)  The name of the tenant to create");
            Console.WriteLine("    -?, -h, --help               Show help and usage information");
            Console.WriteLine();
            Console.WriteLine("create-user:");
            Console.WriteLine("  Description:");
            Console.WriteLine("    Creates a new user within a specified tenant");
            Console.WriteLine();
            Console.WriteLine("  Usage:");
            Console.WriteLine("    dotnet run create-user [options]");
            Console.WriteLine();
            Console.WriteLine("  Options:");
            Console.WriteLine("    -t, --tenant <tenant> (REQUIRED)      The tenant ID where the user will be created");
            Console.WriteLine("    -n, --name <name> (REQUIRED)          The username for the new user");
            Console.WriteLine("    -p, --password <password> (REQUIRED)  The password for the new user");
            Console.WriteLine("    -a, --isAdmin                         Whether the user should have admin privileges [default: False]");
            Console.WriteLine("    -s, --storageLimit <storageLimit>     Storage limit in bytes (e.g., '10gb', '1mb', '1024') [default: 1gb]");
            Console.WriteLine("    -?, -h, --help                        Show help and usage information");
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
            Console.WriteLine("  Environment=JWT_SIGNING_SECRET=your-secret-here-min-32-chars");
            Console.WriteLine("  Environment=BASE_STORAGE_PATH=/var/lib/easyreasy/storage");
            Console.WriteLine();
            Console.WriteLine("Help:");
            Console.WriteLine("  -h, --help, -help, /h, /help  Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run create-tenant --name my-tenant");
            Console.WriteLine("  dotnet run create-user --tenant my-tenant --name john --password mypassword");
            Console.WriteLine("  dotnet run create-user -t my-tenant -n admin -p adminpass -a -s 2gb");
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