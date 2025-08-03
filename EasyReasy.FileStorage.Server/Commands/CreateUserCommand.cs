using EasyReasy.Auth;
using EasyReasy.FileStorage.Remote.Common;
using EasyReasy.FileStorage.Server.Services;
using System.CommandLine;

namespace EasyReasy.FileStorage.Server.Commands
{
    /// <summary>
    /// Command to create a new user within a specified tenant.
    /// </summary>
    public class CreateUserCommand : Command
    {
        public CreateUserCommand() : base("create-user", "Creates a new user within a specified tenant")
        {
            Option<string> tenantOption = new Option<string>(
                aliases: new[] { "--tenant", "-t" },
                description: "The tenant ID where the user will be created")
            {
                IsRequired = true,
            };

            Option<string> nameOption = new Option<string>(
                aliases: new[] { "--name", "-n" },
                description: "The username for the new user")
            {
                IsRequired = true,
            };

            Option<string> passwordOption = new Option<string>(
                aliases: new[] { "--password", "-p" },
                description: "The password for the new user")
            {
                IsRequired = true,
            };

            Option<bool> isAdminOption = new Option<bool>(
                aliases: new[] { "--isAdmin", "-a" },
                description: "Whether the user should have admin privileges",
                getDefaultValue: () => false);

            Option<string> storageLimitOption = new Option<string>(
                aliases: new[] { "--storageLimit", "-s" },
                description: "Storage limit in bytes (e.g., '10gb', '1mb', '1024')",
                getDefaultValue: () => "1gb");

            AddOption(tenantOption);
            AddOption(nameOption);
            AddOption(passwordOption);
            AddOption(isAdminOption);
            AddOption(storageLimitOption);

            this.SetHandler(async (string tenant, string name, string password, bool isAdmin, string storageLimit) =>
            {
                await ExecuteAsync(tenant, name, password, isAdmin, storageLimit);
            }, tenantOption, nameOption, passwordOption, isAdminOption, storageLimitOption);
        }

        private async Task ExecuteAsync(string tenant, string name, string password, bool isAdmin, string storageLimit)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenant) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Error: Tenant, name, and password are required.");
                    return;
                }

                long storageLimitBytes = ParseStorageLimit(storageLimit);
                if (storageLimitBytes <= 0)
                {
                    Console.WriteLine("Error: Invalid storage limit format. Use formats like '10gb', '1mb', or '1024'.");
                    return;
                }

                // Create password hasher
                IPasswordHasher passwordHasher = new SecurePasswordHasher();

                // Create user service for the tenant
                FileSystemUserService userService = new FileSystemUserService(passwordHasher, tenant);

                // Check if user already exists
                User? existingUser = await userService.GetUserByNameAsync(name);
                if (existingUser is not null)
                {
                    Console.WriteLine($"Error: User '{name}' already exists in tenant '{tenant}'.");
                    return;
                }

                // Create the user
                bool success = await userService.CreateUserAsync(name, password, isAdmin, storageLimitBytes);
                if (success)
                {
                    Console.WriteLine($"Successfully created user '{name}' in tenant '{tenant}'");
                    Console.WriteLine($"  Admin: {isAdmin}");
                    Console.WriteLine($"  Storage Limit: {FormatBytes(storageLimitBytes)}");
                }
                else
                {
                    Console.WriteLine($"Error: Failed to create user '{name}' in tenant '{tenant}'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");
            }
        }

        private static long ParseStorageLimit(string storageLimit)
        {
            if (string.IsNullOrWhiteSpace(storageLimit))
            {
                return 1024 * 1024 * 1024; // 1GB default
            }

            storageLimit = storageLimit.ToLowerInvariant().Trim();

            if (storageLimit.EndsWith("gb"))
            {
                string numberPart = storageLimit[..^2];
                if (long.TryParse(numberPart, out long gb))
                {
                    return gb * 1024 * 1024 * 1024;
                }
            }
            else if (storageLimit.EndsWith("mb"))
            {
                string numberPart = storageLimit[..^2];
                if (long.TryParse(numberPart, out long mb))
                {
                    return mb * 1024 * 1024;
                }
            }
            else if (storageLimit.EndsWith("kb"))
            {
                string numberPart = storageLimit[..^2];
                if (long.TryParse(numberPart, out long kb))
                {
                    return kb * 1024;
                }
            }
            else if (long.TryParse(storageLimit, out long bytes))
            {
                return bytes;
            }

            return -1; // Invalid format
        }

        private static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}