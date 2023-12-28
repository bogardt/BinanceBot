using BinanceBot.Abstraction;
using BinanceBot.Utils;
using Moq;

namespace BinanceBot.Tests
{
    [TestClass]
    public class HelperTests
    {
        private Mock<IFileSystem> _mockFileSystem;
        private Helper _helper;

        [TestInitialize]
        public void Setup()
        {
            _mockFileSystem = new Mock<IFileSystem>();
            _helper = new Helper(_mockFileSystem.Object);
        }

        [TestMethod]
        public void GetSolutionPath_ReturnsCorrectPath()
        {
            // Arrange
            var fakeCurrentDirectory = "C:\\Projects\\MySolution";
            var fakeSolutionFile = new FileInfo("MySolution.sln");
            _mockFileSystem.Setup(fs => fs.GetCurrentDirectory()).Returns(fakeCurrentDirectory);
            _mockFileSystem.Setup(fs => fs.GetFiles(fakeCurrentDirectory, "*.sln"))
                           .Returns(new[] { fakeSolutionFile });

            // Act
            var result = _helper.GetSolutionPath();

            // Assert
            Assert.AreEqual(fakeCurrentDirectory, result);
        }

        [TestMethod]
        public void GetSolutionPath_ReturnsNull_WhenSolutionFileNotFound()
        {
            // Arrange
            var fakeCurrentDirectory = "C:\\Projects\\MySolution";
            _mockFileSystem.Setup(fs => fs.GetCurrentDirectory()).Returns(fakeCurrentDirectory);
            _mockFileSystem.Setup(fs => fs.GetFiles(fakeCurrentDirectory, "*.sln"))
                           .Returns(new FileInfo[0]);

            // Act
            var result = _helper.GetSolutionPath();

            // Assert
            Assert.IsNull(result);
        }

    }
}