using System.Text;

namespace EasyReasy.FileStorage.Tests
{
    [TestClass]
    public class LocalFileSystemTests
    {
        private string _testBasePath = null!;
        private LocalFileSystem _fileSystem = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _testBasePath = InitializeTestFileSystem();
            _fileSystem = new LocalFileSystem(_testBasePath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(_testBasePath))
            {
                Directory.Delete(_testBasePath, true);
            }
        }

        private string InitializeTestFileSystem()
        {
            // Create a unique test directory
            string testDirectory = Path.Combine(Path.GetTempPath(), $"EasyReasyTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(testDirectory);

            // Create test files and directories for specific tests
            CreateTestFilesForDeleteTest(testDirectory);
            CreateTestFilesForCopyTest(testDirectory);
            CreateTestFilesForEnumerationTest(testDirectory);
            CreateTestFilesForTextOperationsTest(testDirectory);

            return testDirectory;
        }

        private void CreateTestFilesForDeleteTest(string testDirectory)
        {
            string deleteTestDirectory = Path.Combine(testDirectory, "DeleteTest");
            Directory.CreateDirectory(deleteTestDirectory);

            // File to delete
            File.WriteAllText(Path.Combine(deleteTestDirectory, "file-to-delete.txt"), "content");

            // Directory to delete
            string dirToDelete = Path.Combine(deleteTestDirectory, "dir-to-delete");
            Directory.CreateDirectory(dirToDelete);
            File.WriteAllText(Path.Combine(dirToDelete, "nested-file.txt"), "content");
        }

        private void CreateTestFilesForCopyTest(string testDirectory)
        {
            string copyTestDirectory = Path.Combine(testDirectory, "CopyTest");
            Directory.CreateDirectory(copyTestDirectory);

            // Source file for copy test
            File.WriteAllText(Path.Combine(copyTestDirectory, "source-file.txt"), "source content");
        }

        private void CreateTestFilesForEnumerationTest(string testDirectory)
        {
            string enumTestDirectory = Path.Combine(testDirectory, "EnumTest");
            Directory.CreateDirectory(enumTestDirectory);

            // Create multiple files for enumeration
            File.WriteAllText(Path.Combine(enumTestDirectory, "file1.txt"), "content1");
            File.WriteAllText(Path.Combine(enumTestDirectory, "file2.txt"), "content2");
            File.WriteAllText(Path.Combine(enumTestDirectory, "file3.txt"), "content3");
        }

        private void CreateTestFilesForTextOperationsTest(string testDirectory)
        {
            string textTestDirectory = Path.Combine(testDirectory, "TextTest");
            Directory.CreateDirectory(textTestDirectory);

            // File for text operations
            File.WriteAllText(Path.Combine(textTestDirectory, "existing-file.txt"), "existing content");
        }

        [TestMethod]
        public async Task WriteFileAsTextAsync_ShouldCreateFile()
        {
            // Arrange
            string filePath = "WriteTest/test-file.txt";
            string content = "Hello, World!";

            // Act
            await _fileSystem.WriteFileAsTextAsync(filePath, content);

            // Assert
            Assert.IsTrue(await _fileSystem.FileExistsAsync(filePath));
            string readContent = await _fileSystem.ReadFileAsTextAsync(filePath);
            Assert.AreEqual(content, readContent);
        }

        [TestMethod]
        public async Task ReadFileAsTextAsync_ShouldReturnCorrectContent()
        {
            // Arrange
            string filePath = "TextTest/existing-file.txt";
            string expectedContent = "existing content";

            // Act
            string actualContent = await _fileSystem.ReadFileAsTextAsync(filePath);

            // Assert
            Assert.AreEqual(expectedContent, actualContent);
        }

        [TestMethod]
        public async Task DeleteFileAsync_ShouldDeleteFile()
        {
            // Arrange
            string filePath = "DeleteTest/file-to-delete.txt";
            Assert.IsTrue(await _fileSystem.FileExistsAsync(filePath));

            // Act
            await _fileSystem.DeleteFileAsync(filePath);

            // Assert
            Assert.IsFalse(await _fileSystem.FileExistsAsync(filePath));
        }

        [TestMethod]
        public async Task DeleteDirectoryAsync_ShouldDeleteDirectory()
        {
            // Arrange
            string dirPath = "DeleteTest/dir-to-delete";
            Assert.IsTrue(await _fileSystem.DirectoryExistsAsync(dirPath));

            // Act
            await _fileSystem.DeleteDirectoryAsync(dirPath, true);

            // Assert
            Assert.IsFalse(await _fileSystem.DirectoryExistsAsync(dirPath));
        }

        [TestMethod]
        public async Task CopyFileAsync_ShouldCopyFile()
        {
            // Arrange
            string sourcePath = "CopyTest/source-file.txt";
            string destPath = "CopyTest/dest-file.txt";
            Assert.IsTrue(await _fileSystem.FileExistsAsync(sourcePath));

            // Act
            await _fileSystem.CopyFileAsync(sourcePath, destPath);

            // Assert
            Assert.IsTrue(await _fileSystem.FileExistsAsync(destPath));
            string sourceContent = await _fileSystem.ReadFileAsTextAsync(sourcePath);
            string destContent = await _fileSystem.ReadFileAsTextAsync(destPath);
            Assert.AreEqual(sourceContent, destContent);
        }

        [TestMethod]
        public async Task EnumerateFilesAsync_ShouldReturnAllFiles()
        {
            // Arrange
            string dirPath = "EnumTest";

            // Act
            IEnumerable<string> files = await _fileSystem.EnumerateFilesAsync(dirPath);

            // Assert
            List<string> fileList = files.ToList();
            Assert.AreEqual(3, fileList.Count);
            Assert.IsTrue(fileList.Any(f => f.EndsWith("file1.txt")));
            Assert.IsTrue(fileList.Any(f => f.EndsWith("file2.txt")));
            Assert.IsTrue(fileList.Any(f => f.EndsWith("file3.txt")));
        }

        [TestMethod]
        public async Task GetFileSizeAsync_ShouldReturnCorrectSize()
        {
            // Arrange
            string filePath = "TextTest/existing-file.txt";
            string content = "existing content";
            long expectedSize = Encoding.UTF8.GetByteCount(content);

            // Act
            long actualSize = await _fileSystem.GetFileSizeAsync(filePath);

            // Assert
            Assert.AreEqual(expectedSize, actualSize);
        }

        [TestMethod]
        public async Task GetLastModifiedAsync_ShouldReturnValidDateTime()
        {
            // Arrange
            string filePath = "TextTest/existing-file.txt";

            // Act
            DateTime lastModified = await _fileSystem.GetLastModifiedAsync(filePath);

            // Assert
            Assert.IsTrue(lastModified > DateTime.MinValue);
            Assert.IsTrue(lastModified <= DateTime.Now);
        }

        [TestMethod]
        public async Task OpenFileForReadingAsync_ShouldReturnStream()
        {
            // Arrange
            string filePath = "TextTest/existing-file.txt";

            // Act
            using Stream stream = await _fileSystem.OpenFileForReadingAsync(filePath);

            // Assert
            Assert.IsNotNull(stream);
            Assert.IsTrue(stream.CanRead);
            Assert.IsFalse(stream.CanWrite);
        }

        [TestMethod]
        public async Task OpenFileForWritingAsync_ShouldReturnStream()
        {
            // Arrange
            string filePath = "WriteTest/new-file.txt";

            // Act
            using Stream stream = await _fileSystem.OpenFileForWritingAsync(filePath);

            // Assert
            Assert.IsNotNull(stream);
            Assert.IsTrue(stream.CanWrite);
            Assert.IsFalse(stream.CanRead);
        }

        [TestMethod]
        public async Task OpenFileForWritingAsync_WithAppend_ShouldAppendToExistingFile()
        {
            // Arrange
            string filePath = "WriteTest/append-test.txt";
            string initialContent = "Hello";
            string appendContent = " World";

            // Write initial content
            await _fileSystem.WriteFileAsTextAsync(filePath, initialContent);

            // Act - Append content
            using (Stream stream = await _fileSystem.OpenFileForWritingAsync(filePath, append: true))
            {
                byte[] appendBytes = System.Text.Encoding.UTF8.GetBytes(appendContent);
                await stream.WriteAsync(appendBytes, 0, appendBytes.Length);
            }

            // Assert
            string finalContent = await _fileSystem.ReadFileAsTextAsync(filePath);
            Assert.AreEqual("Hello World", finalContent);
        }

        [TestMethod]
        public async Task OpenFileForWritingAsync_WithoutAppend_ShouldOverwriteExistingFile()
        {
            // Arrange
            string filePath = "WriteTest/overwrite-test.txt";
            string initialContent = "Hello World";
            string newContent = "Goodbye";

            // Write initial content
            await _fileSystem.WriteFileAsTextAsync(filePath, initialContent);

            // Act - Overwrite content
            using (Stream stream = await _fileSystem.OpenFileForWritingAsync(filePath, append: false))
            {
                byte[] newBytes = System.Text.Encoding.UTF8.GetBytes(newContent);
                await stream.WriteAsync(newBytes, 0, newBytes.Length);
            }

            // Assert
            string finalContent = await _fileSystem.ReadFileAsTextAsync(filePath);
            Assert.AreEqual("Goodbye", finalContent);
        }

        [TestMethod]
        public async Task OpenFileForWritingAsync_ShouldWriteContentViaStream()
        {
            // Arrange
            string filePath = "WriteTest/stream-write-test.txt";
            string expectedContent = "Hello from stream!";
            byte[] contentBytes = Encoding.UTF8.GetBytes(expectedContent);

            // Act
            using (Stream writeStream = await _fileSystem.OpenFileForWritingAsync(filePath))
            {
                await writeStream.WriteAsync(contentBytes, 0, contentBytes.Length);
                await writeStream.FlushAsync();
            }

            // Assert
            Assert.IsTrue(await _fileSystem.FileExistsAsync(filePath));
            string actualContent = await _fileSystem.ReadFileAsTextAsync(filePath);
            Assert.AreEqual(expectedContent, actualContent);
        }

        [TestMethod]
        public async Task CreateDirectoryAsync_ShouldCreateDirectory()
        {
            // Arrange
            string dirPath = "NewDirectory";

            // Act
            await _fileSystem.CreateDirectoryAsync(dirPath);

            // Assert
            Assert.IsTrue(await _fileSystem.DirectoryExistsAsync(dirPath));
        }

        [TestMethod]
        public void Constructor_WithEmptyBasePath_ShouldAllowAbsolutePaths()
        {
            // Arrange & Act
            LocalFileSystem fileSystem = new LocalFileSystem(string.Empty);

            // Assert - should not throw when using absolute paths
            Assert.IsNotNull(fileSystem);
        }

        [TestMethod]
        public void Constructor_WithBasePath_ShouldPreventDirectoryTraversal()
        {
            // Arrange & Act
            LocalFileSystem fileSystem = new LocalFileSystem(_testBasePath);

            // Assert - should throw when trying to access outside base path
            Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            {
                await fileSystem.WriteFileAsTextAsync("../../../outside-file.txt", "content");
            });
        }
    }
}