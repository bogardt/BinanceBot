using BinanceBot.Utils;

namespace BinanceBot.Tests.Utils
{
    [TestClass]
    public class FileSystemTests
    {
        private FileSystem _fileSystem = new();

        [TestMethod]
        public void GetCurrentDirectory_ReturnsCorrectDirectory()
        {
            // Act
            var currentDirectory = _fileSystem.GetCurrentDirectory();

            // Assert
            Assert.AreEqual(Directory.GetCurrentDirectory(), currentDirectory);
        }

        [TestMethod]
        public void GetFiles_ReturnsCorrectFiles()
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
        public void GetFiles_InvalidPath_ThrowsException()
        {
            // Arrange
            var invalidPath = "Z:\\NonExistentDirectory";

            // Act & Assert
            Assert.ThrowsException<DirectoryNotFoundException>(
                () => _fileSystem.GetFiles(invalidPath, "*.txt")
            );
        }

        [TestMethod]
        public void GetCurrentDirectory_ReturnsCurrentDirectory()
        {
            var expected = Directory.GetCurrentDirectory();
            var actual = _fileSystem.GetCurrentDirectory();
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetDirectoryName_ValidPath_ReturnsDirectoryName()
        {
            var path = "C:\\test\\myfile.txt";
            var expected = Path.GetDirectoryName(path);
            var actual = _fileSystem.GetDirectoryName(path);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DirectoryExists_ExistingDirectory_ReturnsTrue()
        {
            var path = Directory.GetCurrentDirectory();
            var actual = _fileSystem.DirectoryExists(path);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void DirectoryExists_NonExistingDirectory_ReturnsFalse()
        {
            var path = "C:\\NonExisting\\Path";
            var actual = _fileSystem.DirectoryExists(path);
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void CreateDirectory_CreateNewDirectory_ReturnsDirectoryInfo()
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
        public void GetFiles_DirectoryWithoutMatchingFiles_ReturnsNoFiles()
        {
            var path = Directory.GetCurrentDirectory();
            var pattern = "*.abcdef";
            var files = _fileSystem.GetFiles(path, pattern);
            Assert.IsTrue(files.Length == 0);
        }
    }
}
