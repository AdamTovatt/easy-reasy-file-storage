using EasyReasy.Auth;
using EasyReasy.FileStorage.Remote.Common;
using EasyReasy.FileStorage.Server.Services;

namespace EasyReasy.FileStorage.Tests
{
    [TestClass]
    public class FileSystemUserServiceTests
    {
        private string _testBasePath = null!;
        private FileSystemUserService _userService = null!;
        private IPasswordHasher _passwordHasher = null!;
        private string _testTenantId = "test-tenant";

        [TestInitialize]
        public void TestInitialize()
        {
            _passwordHasher = new SecurePasswordHasher();
            _testBasePath = InitializeTestFileSystem();

            // Set the environment variable for the test
            Environment.SetEnvironmentVariable("BASE_STORAGE_PATH", _testBasePath);

            _userService = new FileSystemUserService(_passwordHasher, _testTenantId);
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

            // Create test users within the tenant directory
            CreateTestUser(testDirectory, _testTenantId, "testuser1", "password123");
            CreateTestUser(testDirectory, _testTenantId, "testuser2", "password456");
            CreateTestUser(testDirectory, _testTenantId, "admin", "admin123");

            return testDirectory;
        }

        private void CreateTestUser(string testDirectory, string tenantId, string username, string password)
        {
            string userDirectoryPath = Path.Combine(testDirectory, tenantId, username);
            string passwordFilePath = Path.Combine(userDirectoryPath, ".password");
            string filesDirectoryPath = Path.Combine(userDirectoryPath, "files");

            // Create user directory
            Directory.CreateDirectory(userDirectoryPath);

            // Create files directory
            Directory.CreateDirectory(filesDirectoryPath);

            // Create password file with hashed password using the proper password hasher
            string passwordHash = _passwordHasher.HashPassword(password, username);
            File.WriteAllText(passwordFilePath, passwordHash);
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
            string filesDirectoryPath = Path.Combine(_testBasePath, _testTenantId, username, "files");
            Assert.IsTrue(Directory.Exists(filesDirectoryPath));
        }

        [TestMethod]
        public async Task CreateUserAsync_ShouldCreatePasswordFile()
        {
            // Arrange
            string username = "passworduser";
            string password = "password";

            // Act
            bool success = await _userService.CreateUserAsync(username, password);

            // Assert
            Assert.IsTrue(success);

            // Verify password file was created
            string passwordFilePath = Path.Combine(_testBasePath, _testTenantId, username, ".password");
            Assert.IsTrue(File.Exists(passwordFilePath));

            // Verify password file contains hashed password
            string passwordHash = File.ReadAllText(passwordFilePath);
            Assert.IsFalse(string.IsNullOrEmpty(passwordHash));

            IPasswordHasher passwordHasher = new SecurePasswordHasher();
            Assert.IsTrue(passwordHasher.ValidatePassword(password, passwordHash, username));
        }
    }
}