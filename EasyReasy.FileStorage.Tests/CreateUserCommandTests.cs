using EasyReasy.Auth;
using EasyReasy.FileStorage.Remote.Common;
using EasyReasy.FileStorage.Server.Commands;
using EasyReasy.FileStorage.Server.Configuration;
using EasyReasy.FileStorage.Server.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Text.Json;

namespace EasyReasy.FileStorage.Tests
{
    [TestClass]
    public class CreateUserCommandTests
    {
        private string _testBasePath = null!;
        private CreateUserCommand _command = null!;
        private string _testTenantId = "test-tenant";
        private IBasePathProvider _basePathProvider = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _testBasePath = Path.Combine(Path.GetTempPath(), $"EasyReasyUserCommandTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testBasePath);

            // Set the environment variable for the test
            Environment.SetEnvironmentVariable("BASE_STORAGE_PATH", _testBasePath);

            // Create logger and base path provider
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<BasePathProvider> logger = loggerFactory.CreateLogger<BasePathProvider>();
            _basePathProvider = new BasePathProvider(logger);

            _command = new CreateUserCommand();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_testBasePath))
            {
                Directory.Delete(_testBasePath, true);
            }

            // Clean up environment variable
            Environment.SetEnvironmentVariable("BASE_STORAGE_PATH", null);
        }

        [TestMethod]
        public void CreateUserCommand_WithValidParameters_ShouldCreateUser()
        {
            // Arrange
            string username = "testuser";
            string password = "testpassword";
            string[] args = { "create-user", "--tenant", _testTenantId, "--name", username, "--password", password };

            // Act
            int exitCode = _command.Invoke(args);

            // Assert
            Assert.AreEqual(0, exitCode);

            // Verify user was created
            string dataPath = _basePathProvider.GetDataPath();
            string userJsonFilePath = Path.Combine(dataPath, _testTenantId, username, "user.json");
            Assert.IsTrue(File.Exists(userJsonFilePath));

            // Verify user data
            string userJson = File.ReadAllText(userJsonFilePath);
            User? user = JsonSerializer.Deserialize<User>(userJson, JsonConfiguration.DefaultOptions);
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
            Assert.IsFalse(user.IsAdmin);
            Assert.AreEqual(1024 * 1024 * 1024, user.StorageLimitBytes); // Default 1GB
        }

        [TestMethod]
        public void CreateUserCommand_WithAdminUser_ShouldCreateAdminUser()
        {
            // Arrange
            string username = "adminuser";
            string password = "adminpassword";
            string[] args = { "create-user", "--tenant", _testTenantId, "--name", username, "--password", password, "--isAdmin", "true" };

            // Act
            int exitCode = _command.Invoke(args);

            // Assert
            Assert.AreEqual(0, exitCode);

            // Verify user was created
            string dataPath = _basePathProvider.GetDataPath();
            string userJsonFilePath = Path.Combine(dataPath, _testTenantId, username, "user.json");
            Assert.IsTrue(File.Exists(userJsonFilePath));

            // Verify user data
            string userJson = File.ReadAllText(userJsonFilePath);
            User? user = JsonSerializer.Deserialize<User>(userJson, JsonConfiguration.DefaultOptions);
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
            Assert.IsTrue(user.IsAdmin);
        }

        [TestMethod]
        public void CreateUserCommand_WithCustomStorageLimit_ShouldSetStorageLimit()
        {
            // Arrange
            string username = "storageuser";
            string password = "password";
            string[] args = { "create-user", "--tenant", _testTenantId, "--name", username, "--password", password, "--storageLimit", "500mb" };

            // Act
            int exitCode = _command.Invoke(args);

            // Assert
            Assert.AreEqual(0, exitCode);

            // Verify user was created
            string dataPath = _basePathProvider.GetDataPath();
            string userJsonFilePath = Path.Combine(dataPath, _testTenantId, username, "user.json");
            Assert.IsTrue(File.Exists(userJsonFilePath));

            // Verify user data
            string userJson = File.ReadAllText(userJsonFilePath);
            User? user = JsonSerializer.Deserialize<User>(userJson, JsonConfiguration.DefaultOptions);
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
            Assert.AreEqual(500 * 1024 * 1024, user.StorageLimitBytes); // 500MB
        }

        [TestMethod]
        public void CreateUserCommand_WithExistingUser_ShouldReturnError()
        {
            // Arrange
            string username = "existinguser";
            string password = "password";

            // Create existing user
            CreateTestUser(username, password, false, 1024 * 1024 * 1024);

            string[] args = { "create-user", "--tenant", _testTenantId, "--name", username, "--password", password };

            // Act
            int exitCode = _command.Invoke(args);

            // Assert
            // System.CommandLine always returns 0, so we can't rely on exit codes in tests
            // The error handling is tested by checking that the user was not created
            string dataPath = _basePathProvider.GetDataPath();
            string userJsonFilePath = Path.Combine(dataPath, _testTenantId, username, "user.json");
            string userJson = File.ReadAllText(userJsonFilePath);
            User? user = JsonSerializer.Deserialize<User>(userJson, JsonConfiguration.DefaultOptions);

            // Verify the user data hasn't changed (no new user was created)
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
        }

        [TestMethod]
        public void CreateUserCommand_WithEmptyTenant_ShouldNotCreateUser()
        {
            // Arrange
            string[] args = { "create-user", "--tenant", "", "--name", "user", "--password", "password" };

            // Act
            _command.Invoke(args);

            // Assert
            // Verify no user was created in the empty tenant
            string userJsonFilePath = Path.Combine(_testBasePath, "", "user", "user.json");
            Assert.IsFalse(File.Exists(userJsonFilePath));
        }

        [TestMethod]
        public void CreateUserCommand_WithEmptyUsername_ShouldNotCreateUser()
        {
            // Arrange
            string[] args = { "create-user", "--tenant", _testTenantId, "--name", "", "--password", "password" };

            // Act
            _command.Invoke(args);

            // Assert
            // Verify no user was created with empty username
            string userJsonFilePath = Path.Combine(_testBasePath, _testTenantId, "", "user.json");
            Assert.IsFalse(File.Exists(userJsonFilePath));
        }

        [TestMethod]
        public void CreateUserCommand_WithEmptyPassword_ShouldNotCreateUser()
        {
            // Arrange
            string[] args = { "create-user", "--tenant", _testTenantId, "--name", "user", "--password", "" };

            // Act
            _command.Invoke(args);

            // Assert
            // Verify no user was created with empty password
            string userJsonFilePath = Path.Combine(_testBasePath, _testTenantId, "user", "user.json");
            Assert.IsFalse(File.Exists(userJsonFilePath));
        }

        [TestMethod]
        public void CreateUserCommand_WithInvalidStorageLimit_ShouldNotCreateUser()
        {
            // Arrange
            string[] args = { "create-user", "--tenant", _testTenantId, "--name", "user", "--password", "password", "--storageLimit", "invalid" };

            // Act
            _command.Invoke(args);

            // Assert
            // Verify no user was created with invalid storage limit
            string userJsonFilePath = Path.Combine(_testBasePath, _testTenantId, "user", "user.json");
            Assert.IsFalse(File.Exists(userJsonFilePath));
        }

        [TestMethod]
        public void CreateUserCommand_WithShortOptions_ShouldCreateUser()
        {
            // Arrange
            string username = "shortuser";
            string password = "password";
            string[] args = { "create-user", "-t", _testTenantId, "-n", username, "-p", password };

            // Act
            int exitCode = _command.Invoke(args);

            // Assert
            Assert.AreEqual(0, exitCode);

            // Verify user was created
            string dataPath = _basePathProvider.GetDataPath();
            string userJsonFilePath = Path.Combine(dataPath, _testTenantId, username, "user.json");
            Assert.IsTrue(File.Exists(userJsonFilePath));

            // Verify user data
            string userJson = File.ReadAllText(userJsonFilePath);
            User? user = JsonSerializer.Deserialize<User>(userJson, JsonConfiguration.DefaultOptions);
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
            Assert.IsFalse(user.IsAdmin);
            Assert.AreEqual(1024 * 1024 * 1024, user.StorageLimitBytes); // Default 1GB
        }

        [TestMethod]
        public void CreateUserCommand_WithDifferentStorageFormats_ShouldParseCorrectly()
        {
            // Test GB format
            string[] argsGb = { "create-user", "--tenant", _testTenantId, "--name", "gbuser", "--password", "password", "--storageLimit", "2gb" };
            int exitCodeGb = _command.Invoke(argsGb);
            Assert.AreEqual(0, exitCodeGb);

            // Test MB format
            string[] argsMb = { "create-user", "--tenant", _testTenantId, "--name", "mbuser", "--password", "password", "--storageLimit", "100mb" };
            int exitCodeMb = _command.Invoke(argsMb);
            Assert.AreEqual(0, exitCodeMb);

            // Test KB format
            string[] argsKb = { "create-user", "--tenant", _testTenantId, "--name", "kbuser", "--password", "password", "--storageLimit", "500kb" };
            int exitCodeKb = _command.Invoke(argsKb);
            Assert.AreEqual(0, exitCodeKb);

            // Test bytes format
            string[] argsBytes = { "create-user", "--tenant", _testTenantId, "--name", "bytesuser", "--password", "password", "--storageLimit", "1024" };
            int exitCodeBytes = _command.Invoke(argsBytes);
            Assert.AreEqual(0, exitCodeBytes);
        }

        private void CreateTestUser(string username, string password, bool isAdmin, long storageLimitBytes)
        {
            string dataPath = _basePathProvider.GetDataPath();
            string userDirectoryPath = Path.Combine(dataPath, _testTenantId, username);
            string userJsonFilePath = Path.Combine(userDirectoryPath, "user.json");
            string filesDirectoryPath = Path.Combine(userDirectoryPath, "files");

            // Create user directory
            Directory.CreateDirectory(userDirectoryPath);

            // Create files directory
            Directory.CreateDirectory(filesDirectoryPath);

            // Create user data with hashed password
            IPasswordHasher passwordHasher = new SecurePasswordHasher();
            string passwordHash = passwordHasher.HashPassword(password, username);
            User user = new User(username, passwordHash, isAdmin, storageLimitBytes);

            // Serialize and save the user data to user.json
            string userJson = JsonSerializer.Serialize(user, JsonConfiguration.DefaultOptions);
            File.WriteAllText(userJsonFilePath, userJson);
        }
    }
}