using EasyReasy.Auth;
using EasyReasy.FileStorage.Remote.Common;
using EasyReasy.FileStorage.Server.Configuration;
using EasyReasy.FileStorage.Server.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EasyReasy.FileStorage.Tests
{
    [TestClass]
    public class FileSystemUserServiceTests
    {
        private string _testBasePath = null!;
        private FileSystemUserService _userService = null!;
        private IPasswordHasher _passwordHasher = null!;
        private string _testTenantId = "test-tenant";
        private IBasePathProvider _basePathProvider = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _passwordHasher = new SecurePasswordHasher();
            _testBasePath = InitializeTestFileSystem();
            string dataFolderPath = Path.Combine(_testBasePath, "data");

            // Set the environment variable for the test
            Environment.SetEnvironmentVariable("BASE_STORAGE_PATH", _testBasePath);

            // Create logger and base path provider
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            ILogger<BasePathProvider> logger = loggerFactory.CreateLogger<BasePathProvider>();
            _basePathProvider = new BasePathProvider(logger);

            _userService = new FileSystemUserService(_passwordHasher, _testTenantId, dataFolderPath);
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

        private string InitializeTestFileSystem()
        {
            // Create a unique test directory
            string testDirectory = Path.Combine(Path.GetTempPath(), $"EasyReasyUserTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(testDirectory);
            
            // Create data subfolder
            string dataFolderPath = Path.Combine(testDirectory, "data");
            Directory.CreateDirectory(dataFolderPath);

            // Create test users within the tenant directory in the data folder
            CreateTestUser(dataFolderPath, _testTenantId, "testuser1", "password123", false, 1024 * 1024 * 100); // 100MB
            CreateTestUser(dataFolderPath, _testTenantId, "testuser2", "password456", false, 1024 * 1024 * 200); // 200MB
            CreateTestUser(dataFolderPath, _testTenantId, "admin", "admin123", true, 1024 * 1024 * 1024); // 1GB admin

            return testDirectory;
        }

        private void CreateTestUser(string testDirectory, string tenantId, string username, string password, bool isAdmin, long storageLimitBytes)
        {
            string userDirectoryPath = Path.Combine(testDirectory, tenantId, username);
            string userJsonFilePath = Path.Combine(userDirectoryPath, "user.json");
            string filesDirectoryPath = Path.Combine(userDirectoryPath, "files");

            // Create user directory
            Directory.CreateDirectory(userDirectoryPath);

            // Create files directory
            Directory.CreateDirectory(filesDirectoryPath);

            // Create user data with hashed password
            string passwordHash = _passwordHasher.HashPassword(password, username);
            User user = new User(username, passwordHash, isAdmin, storageLimitBytes);

            // Serialize and save the user data to user.json
            string userJson = JsonSerializer.Serialize(user, JsonConfiguration.DefaultOptions);
            File.WriteAllText(userJsonFilePath, userJson);
        }

        [TestMethod]
        public async Task GetUserByNameAsync_WithValidUser_ShouldReturnUser()
        {
            // Arrange
            string username = "testuser1";

            // Act
            User? user = await _userService.GetUserByNameAsync(username);

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));
            Assert.IsFalse(user.IsAdmin);
            Assert.AreEqual(1024 * 1024 * 100, user.StorageLimitBytes);
        }

        [TestMethod]
        public async Task GetUserByNameAsync_WithAdminUser_ShouldReturnAdminUser()
        {
            // Arrange
            string username = "admin";

            // Act
            User? user = await _userService.GetUserByNameAsync(username);

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));
            Assert.IsTrue(user.IsAdmin);
            Assert.AreEqual(1024 * 1024 * 1024, user.StorageLimitBytes);
        }

        [TestMethod]
        public async Task GetUserByNameAsync_WithInvalidUser_ShouldReturnNull()
        {
            // Arrange
            string username = "nonexistentuser";

            // Act
            User? user = await _userService.GetUserByNameAsync(username);

            // Assert
            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task GetUserByNameAsync_WithEmptyUsername_ShouldReturnNull()
        {
            // Arrange
            string username = "";

            // Act
            User? user = await _userService.GetUserByNameAsync(username);

            // Assert
            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task GetUserByNameAsync_WithNullUsername_ShouldReturnNull()
        {
            // Arrange
            string? username = null;

            // Act
            User? user = await _userService.GetUserByNameAsync(username!);

            // Assert
            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task GetUserByNameAsync_WithWhitespaceUsername_ShouldReturnNull()
        {
            // Arrange
            string username = "   ";

            // Act
            User? user = await _userService.GetUserByNameAsync(username);

            // Assert
            Assert.IsNull(user);
        }

        [TestMethod]
        public async Task GetUserByNameAsync_WithMultipleUsers_ShouldReturnCorrectUser()
        {
            // Arrange
            string username1 = "testuser1";
            string username2 = "testuser2";

            // Act
            User? user1 = await _userService.GetUserByNameAsync(username1);
            User? user2 = await _userService.GetUserByNameAsync(username2);

            // Assert
            Assert.IsNotNull(user1);
            Assert.AreEqual(username1, user1.Id);
            Assert.IsNotNull(user2);
            Assert.AreEqual(username2, user2.Id);
            Assert.AreNotEqual(user1.PasswordHash, user2.PasswordHash);
            Assert.AreEqual(1024 * 1024 * 100, user1.StorageLimitBytes);
            Assert.AreEqual(1024 * 1024 * 200, user2.StorageLimitBytes);
        }

        [TestMethod]
        public void ValidatePassword_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            string password = "password123";
            string username = "testuser";
            IPasswordHasher passwordHasher = new SecurePasswordHasher();
            string passwordHash = passwordHasher.HashPassword(password, username);

            // Act
            bool isValid = passwordHasher.ValidatePassword(password, passwordHash, username);

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod]
        public void ValidatePassword_WithIncorrectPassword_ShouldReturnFalse()
        {
            // Arrange
            string password = "password123";
            string wrongPassword = "wrongpassword";
            string username = "testuser";
            IPasswordHasher passwordHasher = new SecurePasswordHasher();
            string passwordHash = passwordHasher.HashPassword(password, username);

            // Act
            bool isValid = passwordHasher.ValidatePassword(wrongPassword, passwordHash, username);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidatePassword_WithEmptyPassword_ShouldReturnFalse()
        {
            // Arrange
            string password = "";
            string username = "testuser";
            IPasswordHasher passwordHasher = new SecurePasswordHasher();
            string passwordHash = passwordHasher.HashPassword("somepassword", username);

            // Act
            bool isValid = passwordHasher.ValidatePassword(password, passwordHash, username);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public void ValidatePassword_WithEmptyHash_ShouldReturnFalse()
        {
            // Arrange
            string password = "somepassword";
            string passwordHash = "";
            string username = "testuser";
            IPasswordHasher passwordHasher = new SecurePasswordHasher();

            // Act
            bool isValid = passwordHasher.ValidatePassword(password, passwordHash, username);

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod]
        public async Task CreateUserAsync_WithValidCredentials_ShouldCreateUser()
        {
            // Arrange
            string username = "newuser";
            string password = "newpassword";

            // Act
            bool success = await _userService.CreateUserAsync(username, password);

            // Assert
            Assert.IsTrue(success);

            // Verify user was created
            User? user = await _userService.GetUserByNameAsync(username);
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
            Assert.IsFalse(user.IsAdmin);
            Assert.AreEqual(1024 * 1024 * 1024, user.StorageLimitBytes); // Default 1GB

            // Verify password validation works
            IPasswordHasher passwordHasher = new SecurePasswordHasher();
            Assert.IsTrue(passwordHasher.ValidatePassword(password, user.PasswordHash, username));
        }

        [TestMethod]
        public async Task CreateUserAsync_WithAdminUser_ShouldCreateAdminUser()
        {
            // Arrange
            string username = "newadmin";
            string password = "adminpassword";

            // Act
            bool success = await _userService.CreateUserAsync(username, password, isAdmin: true, storageLimitBytes: 2L * 1024 * 1024 * 1024); // 2GB

            // Assert
            Assert.IsTrue(success);

            // Verify user was created
            User? user = await _userService.GetUserByNameAsync(username);
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);
            Assert.IsTrue(user.IsAdmin);
            Assert.AreEqual(2L * 1024 * 1024 * 1024, user.StorageLimitBytes);

            // Verify password validation works
            IPasswordHasher passwordHasher = new SecurePasswordHasher();
            Assert.IsTrue(passwordHasher.ValidatePassword(password, user.PasswordHash, username));
        }

        [TestMethod]
        public async Task CreateUserAsync_WithEmptyUsername_ShouldReturnFalse()
        {
            // Arrange
            string username = "";
            string password = "password";

            // Act
            bool success = await _userService.CreateUserAsync(username, password);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task CreateUserAsync_WithEmptyPassword_ShouldReturnFalse()
        {
            // Arrange
            string username = "testuser";
            string password = "";

            // Act
            bool success = await _userService.CreateUserAsync(username, password);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task CreateUserAsync_ShouldCreateFilesDirectory()
        {
            // Arrange
            string username = "filesuser";
            string password = "password";

            // Act
            bool success = await _userService.CreateUserAsync(username, password);

            // Assert
            Assert.IsTrue(success);

            // Verify files directory was created
            string dataPath = _basePathProvider.GetDataPath();
            string filesDirectoryPath = Path.Combine(dataPath, _testTenantId, username, "files");
            Assert.IsTrue(Directory.Exists(filesDirectoryPath));
        }

        [TestMethod]
        public async Task CreateUserAsync_ShouldCreateUserJsonFile()
        {
            // Arrange
            string username = "jsonuser";
            string password = "password";

            // Act
            bool success = await _userService.CreateUserAsync(username, password);

            // Assert
            Assert.IsTrue(success);

            // Verify user.json file was created
            string dataPath = _basePathProvider.GetDataPath();
            string userJsonFilePath = Path.Combine(dataPath, _testTenantId, username, "user.json");
            Assert.IsTrue(File.Exists(userJsonFilePath));

            // Verify user.json file contains valid JSON
            string userJson = File.ReadAllText(userJsonFilePath);
            Assert.IsFalse(string.IsNullOrEmpty(userJson));

            // Verify the JSON can be deserialized
            User? user = JsonSerializer.Deserialize<User>(userJson, JsonConfiguration.DefaultOptions);
            Assert.IsNotNull(user);
            Assert.AreEqual(username, user.Id);

            IPasswordHasher passwordHasher = new SecurePasswordHasher();
            Assert.IsTrue(passwordHasher.ValidatePassword(password, user.PasswordHash, username));
        }
    }
}