using EasyReasy.FileStorage.Server.Commands;
using EasyReasy.FileStorage.Server.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace EasyReasy.FileStorage.Tests
{
    [TestClass]
    public class CreateTenantCommandTests
    {
        private string _testBasePath = null!;
        private CreateTenantCommand _command = null!;
        private IBasePathProvider _basePathProvider = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _testBasePath = Path.Combine(Path.GetTempPath(), $"EasyReasyTenantTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testBasePath);

            // Set the environment variable for the test
            Environment.SetEnvironmentVariable("BASE_STORAGE_PATH", _testBasePath);

            // Create logger and base path provider
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<BasePathProvider> logger = loggerFactory.CreateLogger<BasePathProvider>();
            _basePathProvider = new BasePathProvider(logger);

            _command = new CreateTenantCommand();
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
        public void CreateTenantCommand_WithValidName_ShouldCreateTenantDirectory()
        {
            // Arrange
            string tenantName = "test-tenant";
            string[] args = { "create-tenant", "--name", tenantName };

            // Act
            int exitCode = _command.Invoke(args);

            // Assert
            Assert.AreEqual(0, exitCode);
            string dataPath = _basePathProvider.GetDataPath();
            string tenantDirectoryPath = Path.Combine(dataPath, tenantName);
            Assert.IsTrue(Directory.Exists(tenantDirectoryPath));
        }

        [TestMethod]
        public void CreateTenantCommand_WithExistingTenant_ShouldNotCreateDuplicate()
        {
            // Arrange
            string tenantName = "existing-tenant";
            string dataPath = _basePathProvider.GetDataPath();
            string tenantDirectoryPath = Path.Combine(dataPath, tenantName);
            Directory.CreateDirectory(tenantDirectoryPath);

            string[] args = { "create-tenant", "--name", tenantName };

            // Act
            _command.Invoke(args);

            // Assert
            // Verify the tenant directory still exists and wasn't recreated
            Assert.IsTrue(Directory.Exists(tenantDirectoryPath));
        }

        [TestMethod]
        public void CreateTenantCommand_WithEmptyName_ShouldNotCreateTenant()
        {
            // Arrange
            string[] args = { "create-tenant", "--name", "" };

            // Act
            _command.Invoke(args);

            // Assert
            // Verify no tenant was created with empty name
            // The command should not create any directory when name is empty
            string dataPath = _basePathProvider.GetDataPath();
            string[] directories = Directory.GetDirectories(dataPath);
            Assert.AreEqual(0, directories.Length, "No directories should be created with empty tenant name");
        }

        [TestMethod]
        public void CreateTenantCommand_WithWhitespaceName_ShouldNotCreateTenant()
        {
            // Arrange
            string[] args = { "create-tenant", "--name", "   " };

            // Act
            _command.Invoke(args);

            // Assert
            // Verify no tenant was created with whitespace name
            // The command should not create any directory when name is whitespace
            string dataPath = _basePathProvider.GetDataPath();
            string[] directories = Directory.GetDirectories(dataPath);
            Assert.AreEqual(0, directories.Length, "No directories should be created with whitespace tenant name");
        }

        [TestMethod]
        public void CreateTenantCommand_WithShortOption_ShouldCreateTenantDirectory()
        {
            // Arrange
            string tenantName = "short-option-tenant";
            string[] args = { "create-tenant", "-n", tenantName };

            // Act
            int exitCode = _command.Invoke(args);

            // Assert
            Assert.AreEqual(0, exitCode);
            string dataPath = _basePathProvider.GetDataPath();
            string tenantDirectoryPath = Path.Combine(dataPath, tenantName);
            Assert.IsTrue(Directory.Exists(tenantDirectoryPath));
        }

        [TestMethod]
        public void CreateTenantCommand_WithMultipleTenants_ShouldCreateAllTenants()
        {
            // Arrange
            string tenantName1 = "tenant1";
            string tenantName2 = "tenant2";
            string[] args1 = { "create-tenant", "--name", tenantName1 };
            string[] args2 = { "create-tenant", "--name", tenantName2 };

            // Act
            int exitCode1 = _command.Invoke(args1);
            int exitCode2 = _command.Invoke(args2);

            // Assert
            Assert.AreEqual(0, exitCode1);
            Assert.AreEqual(0, exitCode2);

            string dataPath = _basePathProvider.GetDataPath();
            string tenantDirectoryPath1 = Path.Combine(dataPath, tenantName1);
            string tenantDirectoryPath2 = Path.Combine(dataPath, tenantName2);
            Assert.IsTrue(Directory.Exists(tenantDirectoryPath1));
            Assert.IsTrue(Directory.Exists(tenantDirectoryPath2));
        }
    }
}