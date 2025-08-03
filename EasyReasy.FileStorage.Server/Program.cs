using EasyReasy.Auth;
using EasyReasy.EnvironmentVariables;
using EasyReasy.FileStorage.Server.Services;

namespace EasyReasy.FileStorage.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Validate environment variables at startup
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariable));

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

            app.Run();
        }
    }
}
