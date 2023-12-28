using BinanceBot.Abstraction;
using BinanceBot.Core;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class TechnicalIndicatorsCalculatorTests
    {
        [TestMethod]
        public void CalculateMovingAverage_ValidKlines_CalculatesAverage()
        {
            // Arrange
            var binanceClientMock = new Mock<IBinanceClient>();
            var calculator = new TechnicalIndicatorsCalculator(binanceClientMock.Object);
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "100", "100.5" },
                new List<object> { "100.5", "100.5", "100.5", "100.5", "200", "100.5" }
            };
            int period = 2;

            // Act
            var result = calculator.CalculateMovingAverage(klines, period);

            // Assert
            Assert.AreEqual(150m, result);
        }

        [TestMethod]
        public void CalculateMovingAverage_InvalidKlines_ThrowsException()
        {
            // Arrange
            var binanceClientMock = new Mock<IBinanceClient>();
            var calculator = new TechnicalIndicatorsCalculator(binanceClientMock.Object);
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "invalid_data", "100.5" },
            };
            int period = 2;

            // Act & Assert
            Assert.ThrowsException<InvalidCastException>(() => calculator.CalculateMovingAverage(klines, period));
        }

        [TestMethod]
        public void CalculateRSI_ValidKlines_CalculatesRSI()
        {
            // Arrange
            var binanceClientMock = new Mock<IBinanceClient>();
            var calculator = new TechnicalIndicatorsCalculator(binanceClientMock.Object);
            var symbol = "BTCUSDT";
            var interval = "1m";
            int period = 60;

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

            binanceClientMock.Setup(c => c.GetKLinesBySymbolAsync(symbol, interval, (period + 1).ToString()))
                             .ReturnsAsync(klines);

            // Act
            var result = calculator.CalculateRSI(klines, period);

            // Assert
            Assert.IsTrue(result < 50);
            Assert.IsTrue(result > 49);
        }

        [TestMethod]
        public void CalculateRSI_ApiThrowsException_ThrowsException()
        {
            // Arrange
            var binanceClientMock = new Mock<IBinanceClient>();
            var calculator = new TechnicalIndicatorsCalculator(binanceClientMock.Object);
            var symbol = "BTCUSDT";
            var interval = "1d";
            int period = 14;
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "invalid_data", "100.5" },
            };

            binanceClientMock.Setup(c => c.GetKLinesBySymbolAsync(symbol, interval, (period + 1).ToString()))
                             .ThrowsAsync(new Exception("API Error"));

            // Act & Assert
            Assert.ThrowsException<InvalidCastException>(() => calculator.CalculateRSI(klines, period));
        }

        [TestMethod]
        public void CalculateRSI_InvalidKlinesData_ThrowsException()
        {
            // Arrange
            var binanceClientMock = new Mock<IBinanceClient>();
            var calculator = new TechnicalIndicatorsCalculator(binanceClientMock.Object);
            var symbol = "BTCUSDT";
            var interval = "1d";
            int period = 14;

            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "invalid_data", "100.5" },
            };

            binanceClientMock.Setup(c => c.GetKLinesBySymbolAsync(symbol, interval, (period + 1).ToString()))
                             .ReturnsAsync(klines);

            // Act & Assert
            Assert.ThrowsException<InvalidCastException>(() => calculator.CalculateRSI(klines, period));
        }
    }
}