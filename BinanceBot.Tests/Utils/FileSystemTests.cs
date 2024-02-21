using BinanceBot.Utils;

namespace BinanceBot.Tests.Utils;

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
        // Arrange
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
        // Arrange
        var expected = Directory.GetCurrentDirectory();

        // Act
        var actual = _fileSystem.GetCurrentDirectory();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GetDirectoryNameValidPathReturnsDirectoryName()
    {
        // Arrange
        var path = "C:\\test\\myfile.txt";
        var expected = Path.GetDirectoryName(path);

        // Act
        var actual = _fileSystem.GetDirectoryName(path);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GetDirectoryNameNullPathThrowsException()
    {
        // Arrange
        var emptyPath = string.Empty;

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(
            () => _fileSystem.GetDirectoryName(emptyPath)
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
        // Arrange
        var path = Directory.GetCurrentDirectory();

        // Act
        var actual = _fileSystem.DirectoryExists(path);

        // Assert
        Assert.IsTrue(actual);
    }

    [TestMethod]
    public void DirectoryExistsNonExistingDirectoryReturnsFalse()
    {
        // Arrange
        var path = "C:\\NonExisting\\Path";

        // Act
        var actual = _fileSystem.DirectoryExists(path);

        // Assert
        Assert.IsFalse(actual);
    }

    [TestMethod]
    public void CreateDirectoryCreateNewDirectoryReturnsDirectoryInfo()
    {
        // Arrange
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestDirectory");
        if (Directory.Exists(path))
        {
            Directory.Delete(path);
        }

        // Act
        var dirInfo = _fileSystem.CreateDirectory(path);

        // Assert
        Assert.IsTrue(dirInfo.Exists && dirInfo.FullName == path);

        Directory.Delete(path);
    }

    [TestMethod]
    public void GetFilesDirectoryWithoutMatchingFilesReturnsNoFiles()
    {
        // Arrange
        var path = Directory.GetCurrentDirectory();
        var pattern = "*.abcdef";

        // Act
        var files = _fileSystem.GetFiles(path, pattern);

        // Assert
        Assert.IsTrue(files.Length == 0);
    }
}
