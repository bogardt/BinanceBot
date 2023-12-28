using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class VolatilityStrategyTests
    {
        private static readonly Dictionary<string, StrategyCurrencyConfiguration> _dict = new()
        {
            { "SOLUSDT", new StrategyCurrencyConfiguration { TargetProfit = 10m, Quantity = 200m, Interval = "1m", Period = 60 } },
        };
        private static readonly string _symbol = "SOLUSDT";

        [TestMethod]
        public void CalculateVolatility_ValidKlines_CalculatesVolatility()
        {
            // Arrange
            var priceRetrieverMock = new Mock<IPriceRetriever>();
            var volatilityStrategy = new VolatilityStrategy(priceRetrieverMock.Object);
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "100.6", "100.5" },
                new List<object> { "100.5", "100.5", "100.5", "100.5", "101.6", "100.5" }
            };
            var expectedPrices = new List<decimal> { 100m, 102m, 98m, 101m, 99m };
            priceRetrieverMock.Setup(pr => pr.GetRecentPrices(klines))
                .Returns(expectedPrices);

            // Act
            var result = volatilityStrategy.CalculateVolatility(klines);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DetermineLossStrategy_GivenVolatilityAndConfig_CalculatesStopLossPrice()
        {
            // Arrange
            var priceRetrieverMock = new Mock<IPriceRetriever>();
            var volatilityStrategy = new VolatilityStrategy(priceRetrieverMock.Object);
            decimal volatility = 0.05m;
            var tradingConfig = new TradingConfig(_dict, _symbol)
            {
                CryptoPurchasePrice = 100m,
                FloorStopLossPercentage = 0.02m,
                CeilingStopLossPercentage = 0.10m,
                VolatilityMultiplier = 2,
                StopLossPercentage = 0.05m
            };

            // Act
            var result = volatilityStrategy.DetermineLossStrategy(volatility, tradingConfig);

            // Assert
            Assert.IsTrue(result < tradingConfig.CryptoPurchasePrice);
        }
    }
}