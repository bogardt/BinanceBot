using BinanceBot.Abstraction;
using BinanceBot.Core;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class PriceRetrieverTests
    {
        private readonly Mock<ILogger> _logger = new();

        [TestMethod]
        public void GetRecentPrices_ValidKlines_ReturnsClosingPrices()
        {
            // Arrange
            var priceRetriever = new PriceRetriever(_logger.Object);
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "100.6", "100.5" },
                new List<object> { "100.5", "100.5", "100.5", "100.5", "101.6", "100.5" }
            };

            // Act
            var result = priceRetriever.GetRecentPrices(klines);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(100.6m, result[0]);
            Assert.AreEqual(101.6m, result[1]);
        }

        [TestMethod]
        public void GetRecentPrices_InvalidKlines_ThrowsException()
        {
            // Arrange
            var priceRetriever = new PriceRetriever(_logger.Object);
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "not a number", "100.5" },
            };

            // Act & Assert
            Assert.ThrowsException<InvalidCastException>(() => priceRetriever.GetRecentPrices(klines));
            _logger.Verify(log => log.WriteLog(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void GetRecentPrices_HttpRequestException_ReturnsEmptyList()
        {
            // Arrange
            var priceRetriever = new PriceRetriever(_logger.Object);
            var klines = new List<List<object>>();

            // Act
            var result = priceRetriever.GetRecentPrices(klines);

            // Assert
            Assert.AreEqual(0, result.Count);
            _logger.Verify(log => log.WriteLog(It.IsAny<string>()), Times.Never);
        }
    }
}