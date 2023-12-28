using BinanceBot.Abstraction;
using BinanceBot.Utils;
using Moq;

namespace BinanceBot.Tests.Utils
{
    [TestClass]
    public class LoggerTests
    {
        private readonly Mock<IFileSystem> _fileSystem = new();
        private readonly Logger _logger;
        public LoggerTests()
        {
            _logger = new Logger(_fileSystem.Object);
        }

        [TestMethod]
        public async Task WriteLogTests()
        {
            _logger.WriteLog("test");
            _logger.WriteLog("toto");
            _logger.WriteLog("tata");
            Assert.IsTrue(File.Exists("./logfile.log"));
            using StreamReader reader = new StreamReader("./logfile.log");
            var content = await reader.ReadToEndAsync();
            Assert.IsTrue(content.Contains("test"));
            Assert.IsTrue(content.Contains("toto"));
            Assert.IsTrue(content.Contains("tata"));
        }

        [TestMethod]
        public async Task WriteLogTests_Console_Output()
        {
            _logger.WriteLog("test");
            Assert.IsTrue(File.Exists("./logfile.log"));
            using StreamReader reader = new StreamReader("./logfile.log");
            var content = await reader.ReadToEndAsync();
            Assert.IsTrue(content.Contains("test"));
        }

        [TestMethod]
        public void WriteLogTests_FolderNotExist()
        {
            // Arrange
            _fileSystem.Setup(f => f.GetDirectoryName(It.IsAny<string>())).Returns("log_directory_path");
            _fileSystem.Setup(f => f.CreateDirectory(It.IsAny<string>())).Returns(It.IsAny<DirectoryInfo>());

            var message = "Test log message";

            // Act
            _logger.WriteLog(message);

            // Assert
            _fileSystem.Verify(f => f.CreateDirectory(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void WriteLog_WhenFileSystemThrowsException_HandlesException()
        {
            // Arrange
            _fileSystem.Setup(fs => fs.GetDirectoryName(It.IsAny<string>()))
                          .Throws(new InvalidOperationException("Test Exception"));

            var outConsole = new StringWriter();
            Console.SetOut(outConsole);

            // Act
            _logger.WriteLog("Test log message");

            // Assert
            string consoleOutput = outConsole.ToString();
            Assert.IsTrue(consoleOutput.Contains("Error writing to log: Test Exception"));
        }
    }
}