using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class VolatilityStrategyTests
    {
        private readonly Mock<IPriceRetriever> _mockPriceRetriever = new();
        private readonly VolatilityStrategy _volatilityStrategy;

        public VolatilityStrategyTests()
        {
            _volatilityStrategy = new VolatilityStrategy(_mockPriceRetriever.Object);
        }

        [TestMethod]
        public void CalculateVolatilityValidKlinesCalculatesVolatility()
        {
            // Arrange
            var klines = new List<List<object>>
            {
                new List<object> { "100.5", "100.5", "100.5", "100.5", "100.6", "100.5" },
                new List<object> { "100.5", "100.5", "100.5", "100.5", "101.6", "100.5" }
            };
            var expectedPrices = new List<decimal> { 100m, 102m, 98m, 101m, 99m };
            _mockPriceRetriever.Setup(pr => pr.GetRecentPrices(klines))
                .Returns(expectedPrices);

            // Act
            var result = _volatilityStrategy.CalculateVolatility(klines);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void DetermineLossStrategyGivenVolatilityAndConfigCalculatesStopLossPrice()
        {
            // Arrange
            decimal volatility = 0.05m;
            var tradingConfig = new TradingConfig(TradeSetup.Dict, TradeSetup.Symbol)
            {
                CryptoPurchasePrice = 100m,
                FloorStopLossPercentage = 0.02m,
                CeilingStopLossPercentage = 0.10m,
                VolatilityMultiplier = 2,
                StopLossPercentage = 0.05m
            };

            // Act
            var result = _volatilityStrategy.DetermineLossStrategy(volatility, tradingConfig);

            // Assert
            Assert.IsTrue(result < tradingConfig.CryptoPurchasePrice);
        }
    }
}