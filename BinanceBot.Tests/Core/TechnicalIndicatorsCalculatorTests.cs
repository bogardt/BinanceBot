using BinanceBot.Abstraction;
using BinanceBot.Core;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class TechnicalIndicatorsCalculatorTests
    {
        private readonly Mock<IPriceRetriever> _mockPriceRetriever = new();
        private readonly TechnicalIndicatorsCalculator _technicalIndicatorsCalculator;

        public TechnicalIndicatorsCalculatorTests()
        {
            _technicalIndicatorsCalculator = new(_mockPriceRetriever.Object);
        }

        [TestMethod]
        public void CalculateMovingAverageValidKlinesCalculatesAverage()
        {
            // Arrange
            var period = 2;
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "100", "100.5" },
                new List<object> { "100.5", "100.5", "100.5", "100.5", "200", "100.5" }
            };

            var mockLogger = new Mock<ILogger>();
            var mockBinanceClient = new Mock<IBinanceClient>();
            var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
            var clothingPrices = priceRetriever.GetClosingPrices(klines);

            _mockPriceRetriever.Setup(c => c.GetClosingPrices(klines)).Returns(clothingPrices);

            // Act
            var result = _technicalIndicatorsCalculator.CalculateMovingAverage(klines, period);

            // Assert
            Assert.AreEqual(150m, result);
        }

        [TestMethod]
        public void CalculateMovingAverageInvalidKlinesThrowsException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockBinanceClient = new Mock<IBinanceClient>();
            var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
            var technicalIndicatorsCalculator = new TechnicalIndicatorsCalculator(priceRetriever);
            var period = 2;
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "invalid_data", "100.5" },
            };

            // Act & Assert
            Assert.ThrowsException<InvalidCastException>(() => technicalIndicatorsCalculator.CalculateMovingAverage(klines, period));
        }

        [TestMethod]
        public void CalculateRSIValidKlinesCalculatesRSI()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockBinanceClient = new Mock<IBinanceClient>();
            var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
            var technicalIndicatorsCalculator = new TechnicalIndicatorsCalculator(priceRetriever);
            var period = 60;
            var klines = CreateLines(period);

            // Act
            var result = technicalIndicatorsCalculator.CalculateRSI(klines, period);

            // Assert
            Assert.IsTrue(result < 50);
            Assert.IsTrue(result > 49);
        }


        [TestMethod]
        public void CalculateRSIApiThrowsExceptionThrowsException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockBinanceClient = new Mock<IBinanceClient>();
            var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
            var technicalIndicatorsCalculator = new TechnicalIndicatorsCalculator(priceRetriever);
            var period = 14;
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "invalid_data", "100.5" },
            };

            // Act & Assert
            Assert.ThrowsException<InvalidCastException>(() => technicalIndicatorsCalculator.CalculateRSI(klines, period));
        }


        private static List<List<object>> CreateLines(int period)
        {
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var kline2 = new List<object> { 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m };
            var klines = new List<List<object>>();

            for (var i = 0; i < period; i++)
            {
                if (i % 2 == 0)
                {
                    klines.Add(kline);
                }
                else
                {
                    klines.Add(kline2);
                }
            }

            return klines;
        }
    }
}