using BinanceBot.Utils;

namespace BinanceBot.Tests
{
    [TestClass]
    public class FileSystemTests
    {
        [TestMethod]
        public void GetCurrentDirectory_ReturnsCorrectDirectory()
        {
            // Arrange
            var fileSystem = new FileSystem();

            // Act
            var currentDirectory = fileSystem.GetCurrentDirectory();

            // Assert
            Assert.AreEqual(Directory.GetCurrentDirectory(), currentDirectory);
        }

        [TestMethod]
        public void GetFiles_ReturnsCorrectFiles()
        {
            // Arrange
            var fileSystem = new FileSystem();
            var currentDirectory = Directory.GetCurrentDirectory();
            var searchPattern = "*.dll";

            // Act
            var files = fileSystem.GetFiles(currentDirectory, searchPattern);

            // Assert
            Assert.IsNotNull(files);
            Assert.IsTrue(files.Length > 0);
            Assert.IsTrue(files.All(file => file.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        public void GetFiles_InvalidPath_ThrowsException()
        {
            // Arrange
            var fileSystem = new FileSystem();
            var invalidPath = "Z:\\NonExistentDirectory";

            // Act & Assert
            Assert.ThrowsException<DirectoryNotFoundException>(
                () => fileSystem.GetFiles(invalidPath, "*.txt")
            );
        }
    }
}
