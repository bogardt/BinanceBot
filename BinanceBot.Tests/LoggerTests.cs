using BinanceBot.Utils;

namespace BinanceBot.Tests
{
    [TestClass]
    public class LoggerTests
    {
        private readonly Logger _logger = new();

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
    }
}