using EasyReasy.Auth;
using EasyReasy.FileStorage.Server.Services;

namespace EasyReasy.FileStorage.Tests
{
    [TestClass]
    public class FileSystemUserServiceFactoryTests
    {
        private string _testBasePath = null!;
        private IPasswordHasher _passwordHasher = null!;
        private FileSystemUserServiceFactory _factory = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _testBasePath = Path.Combine(Path.GetTempPath(), $"EasyReasyFactoryTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(_testBasePath);

            // Set the environment variable for the test
            Environment.SetEnvironmentVariable("BASE_STORAGE_PATH", _testBasePath);

            _passwordHasher = new SecurePasswordHasher();
            _factory = new FileSystemUserServiceFactory(_passwordHasher);
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
        public void CreateUserService_WithValidTenantId_ShouldReturnUserService()
        {
            // Arrange
            string tenantId = "test-tenant";

            // Act
            IUserService userService = _factory.CreateUserService(tenantId);

            // Assert
            Assert.IsNotNull(userService);
            Assert.IsInstanceOfType(userService, typeof(FileSystemUserService));
        }

        [TestMethod]
        public void CreateUserService_WithNullTenantId_ShouldThrowArgumentNullException()
        {
            // Arrange
            string? tenantId = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => _factory.CreateUserService(tenantId!));
        }

        [TestMethod]
        public void CreateUserService_WithEmptyTenantId_ShouldReturnUserService()
        {
            // Arrange
            string tenantId = "";

            // Act
            IUserService userService = _factory.CreateUserService(tenantId);

            // Assert
            Assert.IsNotNull(userService);
            Assert.IsInstanceOfType(userService, typeof(FileSystemUserService));
        }

        [TestMethod]
        public void CreateUserService_WithDifferentTenantIds_ShouldReturnDifferentInstances()
        {
            // Arrange
            string tenantId1 = "tenant1";
            string tenantId2 = "tenant2";

            // Act
            IUserService userService1 = _factory.CreateUserService(tenantId1);
            IUserService userService2 = _factory.CreateUserService(tenantId2);

            // Assert
            Assert.IsNotNull(userService1);
            Assert.IsNotNull(userService2);
            Assert.AreNotSame(userService1, userService2);
        }
    }
}