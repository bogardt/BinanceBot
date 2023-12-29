using BinanceBot.Utils;

namespace BinanceBot.Tests.Utils
{
    [TestClass]
    public class FileSystemTests
    {
        private FileSystem _fileSystem = new();

        [TestMethod]
        public void GetCurrentDirectoryReturnsCorrectDirectory()
        {
            // Act
            var currentDirectory = _fileSystem.GetCurrentDirectory();

            // Assert
            Assert.AreEqual(Directory.GetCurrentDirectory(), currentDirectory);
        }

        [TestMethod]
        public void GetFilesReturnsCorrectFiles()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var searchPattern = "*.dll";

            // Act
            var files = _fileSystem.GetFiles(currentDirectory, searchPattern);

            // Assert
            Assert.IsNotNull(files);
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.All(file => file.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void GetFilesInvalidPathThrowsException()
        {
            // Arrange
            var invalidPath = "Z:\\NonExistentDirectory";

            // Act & Assert
            Assert.ThrowsException<DirectoryNotFoundException>(
                () => _fileSystem.GetFiles(invalidPath, "*.txt")
            );
        }

        [TestMethod]
        public void GetCurrentDirectoryReturnsCurrentDirectory()
        {
            var expected = Directory.GetCurrentDirectory();
            var actual = _fileSystem.GetCurrentDirectory();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirectoryNameValidPathReturnsDirectoryName()
        {
            var path = "C:\\test\\myfile.txt";
            var expected = Path.GetDirectoryName(path);
            var actual = _fileSystem.GetDirectoryName(path);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirectoryNameNullPathThrowsException()
        {
            // Arrange
            string? nullPath = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(
                () => _fileSystem.GetDirectoryName(nullPath)
            );
        }

        [TestMethod]
        public void GetDirectoryNameInvalidPathThrowsException()
        {
            // Arrange
            var invalidPath = "Z:\\";

            // Act & Assert
            Assert.ThrowsException<DirectoryNotFoundException>(
                () => _fileSystem.GetDirectoryName(invalidPath)
            );
        }

        [TestMethod]
        public void DirectoryExistsExistingDirectoryReturnsTrue()
        {
            var path = Directory.GetCurrentDirectory();
            var actual = _fileSystem.DirectoryExists(path);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void DirectoryExistsNonExistingDirectoryReturnsFalse()
        {
            var path = "C:\\NonExisting\\Path";
            var actual = _fileSystem.DirectoryExists(path);
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void CreateDirectoryCreateNewDirectoryReturnsDirectoryInfo()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }

            var dirInfo = _fileSystem.CreateDirectory(path);
            Assert.IsTrue(dirInfo.Exists && dirInfo.FullName == path);
            Directory.Delete(path);
        }

        [TestMethod]
        public void GetFilesDirectoryWithoutMatchingFilesReturnsNoFiles()
        {
            var path = Directory.GetCurrentDirectory();
            var pattern = "*.abcdef";
            var files = _fileSystem.GetFiles(path, pattern);
            Assert.IsTrue(files.Length == 0);
        }
    }
}
