using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using BinanceBot.Strategy;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class MarketTradeHandlerTests
    {
        private readonly MarketTradeHandler _handler;
        private readonly TradeAction _tradeAction;
        private readonly Mock<IBinanceClient> _mockBinanceClient = new();
        private readonly Mock<ITechnicalIndicatorsCalculator> _mockTechnicalIndicatorsCalculator = new();
        private readonly Mock<IVolatilityStrategy> _mockVolatilityStrategy = new();
        private readonly Mock<ILogger> _mockLogger = new();
        private readonly PriceRetriever _priceRetriever = new();
        private static readonly TradingStrategy _tradingStrategy = new()
        {
            TargetProfit = 10m,
            Quantity = 200m,
            Interval = "1m",
            Period = 60,
            Symbol = "SOLUSDT",
            LimitBenefit = 1000,
        };

        public MarketTradeHandlerTests()
        {
            _tradeAction = new TradeAction(_mockBinanceClient.Object, _mockVolatilityStrategy.Object, _priceRetriever, _mockLogger.Object);
            _handler = new MarketTradeHandler(_mockBinanceClient.Object,
                _mockVolatilityStrategy.Object,
                _mockTechnicalIndicatorsCalculator.Object,
                _priceRetriever,
                _tradeAction,
                _mockLogger.Object,
                _tradingStrategy);
        }

        [TestMethod]
        public async Task TradeOnLimitAsyncValidTradeScenario()
        {
            // Arrange
            var interval = _tradingStrategy.Interval;
            var period = _tradingStrategy.Period;
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var klines = Enumerable.Repeat(kline, period).ToList();

            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, period.ToString()))
                              .ReturnsAsync(klines);
            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, (period + 1).ToString()))
                              .ReturnsAsync(klines);

            _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateRSI(klines, period))
                .Returns(30m);

            _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateMovingAverage(klines, period))
                .Returns(100m);

            _mockVolatilityStrategy.Setup(c => c.CalculateVolatility(It.IsAny<List<List<object>>>()))
                .Returns(0.25m);

            var currencyForBuy = new Currency { Symbol = _tradingStrategy.Symbol, Price = 90m };
            var currencyForSell = new Currency { Symbol = _tradingStrategy.Symbol, Price = 100m };
            _mockBinanceClient.SetupSequence(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol))
                              .ReturnsAsync(currencyForBuy)
                              .ReturnsAsync(currencyForSell);

            _mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                              .ReturnsAsync(It.IsAny<TestOrder>());

            var orders = new List<Order>();
            _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol))
                              .ReturnsAsync(orders);

            // Act
            await _handler.TradeOnLimitAsync();

            // Assert
            _mockTechnicalIndicatorsCalculator.Verify(c => c.CalculateRSI(klines, period), Times.Exactly(2));
            _mockVolatilityStrategy.Verify(c => c.CalculateVolatility(It.IsAny<List<List<object>>>()), Times.Exactly(2));
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol), Times.Exactly(2));
            _mockBinanceClient.Verify(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, period.ToString()), Times.Exactly(2));
            _mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol), Times.Exactly(2));
            _mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol), Times.Exactly(2));
            _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(6));
        }

        [TestMethod]
        public async Task TradeOnLimitAsyncWhenApiThrowsExceptionHandlesException()
        {
            // Arrange
            var interval = _tradingStrategy.Interval;
            var period = _tradingStrategy.Period;

            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, period.ToString()))
                              .ReturnsAsync(new List<List<object>>());
            _mockBinanceClient.Setup(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol))
                              .ThrowsAsync(new Exception("API Error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _handler.TradeOnLimitAsync());
            _mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol), Times.Once);
        }
    }
}