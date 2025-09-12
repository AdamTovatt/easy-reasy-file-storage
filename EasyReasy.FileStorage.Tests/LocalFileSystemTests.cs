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
        public async Task OpenFileForWritingAsync_WithAppendMode_ShouldAppendToExistingFile()
        {
            // Arrange
            string filePath = "WriteTest/append-test.txt";
            string initialContent = "Hello";
            string appendContent = " World";

            // Write initial content
            await _fileSystem.WriteFileAsTextAsync(filePath, initialContent);

            // Act - Append content
            using (Stream stream = await _fileSystem.OpenFileForWritingAsync(filePath, FileWriteMode.Append))
            {
                byte[] appendBytes = System.Text.Encoding.UTF8.GetBytes(appendContent);
                await stream.WriteAsync(appendBytes, 0, appendBytes.Length);
            }

            // Assert
            string finalContent = await _fileSystem.ReadFileAsTextAsync(filePath);
            Assert.AreEqual("Hello World", finalContent);
        }

        [TestMethod]
        public async Task OpenFileForWritingAsync_WithOverwriteMode_ShouldOverwriteExistingFile()
        {
            // Arrange
            string filePath = "WriteTest/overwrite-test.txt";
            string initialContent = "Hello World";
            string newContent = "Goodbye";

            // Write initial content
            await _fileSystem.WriteFileAsTextAsync(filePath, initialContent);

            // Act - Overwrite content
            using (Stream stream = await _fileSystem.OpenFileForWritingAsync(filePath, FileWriteMode.Overwrite))
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
        public async Task PreAllocateFileAsync_ShouldCreateFileWithSpecifiedSize()
        {
            // Arrange
            string filePath = "WriteTest/preallocated-file.txt";
            long expectedSize = 1024;

            // Act
            await _fileSystem.PreAllocateFileAsync(filePath, expectedSize);

            // Assert
            Assert.IsTrue(await _fileSystem.FileExistsAsync(filePath));
            long actualSize = await _fileSystem.GetFileSizeAsync(filePath);
            Assert.AreEqual(expectedSize, actualSize);
        }

        [TestMethod]
        public async Task OpenFileForWritingAsync_WithRandomAccessMode_ShouldSupportChunkedUploads()
        {
            // Arrange
            string filePath = "WriteTest/chunked-upload-test.txt";
            long fileSize = 1000;
            
            // Pre-allocate the file
            await _fileSystem.PreAllocateFileAsync(filePath, fileSize);

            // Act - Write chunks at different positions
            byte[] chunk1 = Encoding.UTF8.GetBytes("Hello");
            byte[] chunk2 = Encoding.UTF8.GetBytes("World");
            byte[] chunk3 = Encoding.UTF8.GetBytes("Test");

            // Write chunk 1 at position 0
            using (Stream stream = await _fileSystem.OpenFileForWritingAsync(filePath, FileWriteMode.RandomAccess))
            {
                stream.Seek(0, SeekOrigin.Begin);
                await stream.WriteAsync(chunk1, 0, chunk1.Length);
            }

            // Write chunk 2 at position 500
            using (Stream stream = await _fileSystem.OpenFileForWritingAsync(filePath, FileWriteMode.RandomAccess))
            {
                stream.Seek(500, SeekOrigin.Begin);
                await stream.WriteAsync(chunk2, 0, chunk2.Length);
            }

            // Write chunk 3 at position 200
            using (Stream stream = await _fileSystem.OpenFileForWritingAsync(filePath, FileWriteMode.RandomAccess))
            {
                stream.Seek(200, SeekOrigin.Begin);
                await stream.WriteAsync(chunk3, 0, chunk3.Length);
            }

            // Assert - Verify all chunks are in their correct positions
            using (Stream readStream = await _fileSystem.OpenFileForReadingAsync(filePath))
            {
                // Read chunk 1 (position 0-4)
                readStream.Seek(0, SeekOrigin.Begin);
                byte[] readChunk1 = new byte[chunk1.Length];
                await readStream.ReadAsync(readChunk1, 0, readChunk1.Length);
                Assert.AreEqual("Hello", Encoding.UTF8.GetString(readChunk1));

                // Read chunk 3 (position 200-203)
                readStream.Seek(200, SeekOrigin.Begin);
                byte[] readChunk3 = new byte[chunk3.Length];
                await readStream.ReadAsync(readChunk3, 0, readChunk3.Length);
                Assert.AreEqual("Test", Encoding.UTF8.GetString(readChunk3));

                // Read chunk 2 (position 500-504)
                readStream.Seek(500, SeekOrigin.Begin);
                byte[] readChunk2 = new byte[chunk2.Length];
                await readStream.ReadAsync(readChunk2, 0, readChunk2.Length);
                Assert.AreEqual("World", Encoding.UTF8.GetString(readChunk2));
            }

            // Verify file size is still correct
            long actualSize = await _fileSystem.GetFileSizeAsync(filePath);
            Assert.AreEqual(fileSize, actualSize);
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