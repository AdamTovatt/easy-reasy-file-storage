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
            // Validate environment variables at startup
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariable));

            // Check if we're running in CLI mode (any arguments provided)
            if (args.Length > 0)
            {
                CliApplication cliApplication = new CliApplication();
                return await cliApplication.ExecuteAsync(args);
            }

            // No arguments provided, start the web server
            return await StartWebServerAsync(args);
        }

        private static async Task<int> StartWebServerAsync(string[] args)
        {
            try
            {
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                // Configure EasyReasy.Auth
                string jwtSecret = EnvironmentVariable.JwtSigningSecret.GetValue();
                builder.Services.AddEasyReasyAuth(jwtSecret, issuer: "easy-reasy-file-storage");

                // Register password hasher
                builder.Services.AddSingleton<IPasswordHasher, SecurePasswordHasher>();

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
