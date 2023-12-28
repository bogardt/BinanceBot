using BinanceBot.Abstraction;
using BinanceBot.Core;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class PriceRetrieverTests
    {
        private readonly PriceRetriever _priceRetriever;

        public PriceRetrieverTests()
        {
            _priceRetriever = new PriceRetriever();
        }

        [TestMethod]
        public void GetRecentPrices_ValidKlines_ReturnsClosingPrices()
        {
            // Arrange
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "100.6", "100.5" },
                new List<object> { "100.5", "100.5", "100.5", "100.5", "101.6", "100.5" }
            };

            // Act
            var result = _priceRetriever.GetRecentPrices(klines);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(100.6m, result[0]);
            Assert.AreEqual(101.6m, result[1]);
        }

        [TestMethod]
        public void GetRecentPrices_InvalidKlines_ThrowsException()
        {
            // Arrange
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "not a number", "100.5" },
            };

            // Act & Assert
            Assert.ThrowsException<InvalidCastException>(() => _priceRetriever.GetRecentPrices(klines));
        }

        [TestMethod]
        public void GetRecentPrices_HttpRequestException_ReturnsEmptyList()
        {
            // Arrange
            var klines = new List<List<object>>();

            // Act
            var result = _priceRetriever.GetRecentPrices(klines);

            // Assert
            Assert.AreEqual(0, result.Count);
        }
    }
}