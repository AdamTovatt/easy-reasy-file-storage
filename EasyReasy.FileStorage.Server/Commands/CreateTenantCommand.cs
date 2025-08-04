using EasyReasy.FileStorage.Server.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace EasyReasy.FileStorage.Server.Commands
{
    /// <summary>
    /// Command to create a new tenant directory structure.
    /// </summary>
    public class CreateTenantCommand : Command
    {
        public CreateTenantCommand() : base("create-tenant", "Creates a new tenant directory structure")
        {
            Option<string> nameOption = new Option<string>(
                aliases: new[] { "--name", "-n" },
                description: "The name of the tenant to create")
            {
                IsRequired = true,
            };

            AddOption(nameOption);

            this.SetHandler((string name) =>
            {
                return ExecuteAsync(name);
            }, nameOption);
        }

        private Task ExecuteAsync(string tenantName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantName))
                {
                    Console.WriteLine("Error: Tenant name cannot be empty.");
                    return Task.CompletedTask;
                }

                // Create logger factory and logger
                ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                ILogger<BasePathProvider> logger = loggerFactory.CreateLogger<BasePathProvider>();

                IBasePathProvider basePathProvider = new BasePathProvider(logger);
                string dataPath = basePathProvider.GetDataPath();
                string tenantDirectoryPath = Path.Combine(dataPath, tenantName);

                if (Directory.Exists(tenantDirectoryPath))
                {
                    Console.WriteLine($"Error: Tenant '{tenantName}' already exists.");
                    return Task.CompletedTask;
                }

                Directory.CreateDirectory(tenantDirectoryPath);
                string absoluteTenantPath = Path.GetFullPath(tenantDirectoryPath);
                Console.WriteLine($"Successfully created tenant '{tenantName}' at: {absoluteTenantPath}");

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating tenant: {ex.Message}");
                return Task.CompletedTask;
            }
        }
    }
}