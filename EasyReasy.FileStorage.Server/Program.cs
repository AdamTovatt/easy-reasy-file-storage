using EasyReasy.Auth;
using EasyReasy.EnvironmentVariables;
using EasyReasy.FileStorage.Server.Commands;
using EasyReasy.FileStorage.Server.Services;

namespace EasyReasy.FileStorage.Server
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Check if we're running in CLI mode (any arguments provided)
            if (args.Length > 0)
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

                CliApplication cliApplication = new CliApplication();
                return await cliApplication.ExecuteAsync(args);
            }

            // Validate environment variables at startup for web server mode
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariable));

            // No arguments provided, start the web server
            return await StartWebServerAsync(builder, args);
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

        private static async Task<int> StartWebServerAsync(WebApplicationBuilder builder, string[] args)
        {
            try
            {
                // Configure EasyReasy.Auth
                string jwtSecret = EnvironmentVariable.JwtSigningSecret.GetValue();
                builder.Services.AddEasyReasyAuth(jwtSecret, issuer: "easy-reasy-file-storage");

                // Register password hasher
                builder.Services.AddSingleton<IPasswordHasher, SecurePasswordHasher>();

                // Register base path provider
                builder.Services.AddSingleton<IBasePathProvider, BasePathProvider>();

                // Register user service factory
                builder.Services.AddSingleton<IUserServiceFactory, FileSystemUserServiceFactory>();

                // Register our custom authentication service as singleton
                builder.Services.AddSingleton<IAuthRequestValidationService, AuthService>();

                WebApplication app = builder.Build();

                // Add EasyReasy.Auth middleware
                app.UseEasyReasyAuth();

                // Add authentication endpoints
                app.AddAuthEndpoints(
                    app.Services.GetRequiredService<IAuthRequestValidationService>(),
                    allowApiKeys: false,
                    allowUsernamePassword: true);

                app.MapGet("/", () => "Hello World!");

                await app.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting web server: {ex.Message}");
                return 1;
            }
        }
    }
}
