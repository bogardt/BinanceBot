using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using Moq;

namespace BinanceBot.Tests
{
    [TestClass]
    public class MarketTradeHandlerTests
    {
        private readonly MarketTradeHandler _handler;
        private readonly Mock<IBinanceClient> _mockBinanceClient;
        private readonly Mock<ILogger> _mockLogger;
        private static readonly string _symbol = "SOLUSDT";
        private static readonly Dictionary<string, StrategyCurrencyConfiguration> _dict = new()
        {
            { "SOLUSDT", new StrategyCurrencyConfiguration { TargetProfit = 10m, Quantity = 200m, Interval = "1m", Period = 60 } },
        };

        public MarketTradeHandlerTests()
        {
            _mockBinanceClient = new Mock<IBinanceClient>();
            _mockLogger = new Mock<ILogger>();
            _handler = new MarketTradeHandler(_mockBinanceClient.Object, _mockLogger.Object, new TradingConfig(_dict, _symbol) { LimitBenefit = 1000 });
        }

        [TestMethod]
        public async Task TradeOnLimitAsync_ValidTradeScenario()
        {
            // Arrange
            var interval = _dict[_symbol].Interval;
            var period = _dict[_symbol].Period;
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var klines = Enumerable.Repeat(kline, period).ToList();

            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_symbol, interval, period.ToString()))
                              .ReturnsAsync(klines);
            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_symbol, interval, (period + 1).ToString()))
                              .ReturnsAsync(klines);

            var currencyForBuy = new Currency { Symbol = _symbol, Price = 90m };
            var currencyForSell = new Currency { Symbol = _symbol, Price = 100m };
            _mockBinanceClient.SetupSequence(c => c.GetPriceBySymbolAsync(_symbol))
                              .ReturnsAsync(currencyForBuy)
                              .ReturnsAsync(currencyForSell);

            _mockBinanceClient.Setup(c => c.PlaceOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                              .ReturnsAsync(string.Empty);

            var orders = new List<Order>();
            _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_symbol))
                              .ReturnsAsync(orders);

            // Act
            await _handler.TradeOnLimitAsync();

            // Assert
            _mockBinanceClient.Verify(c => c.GetKLinesBySymbolAsync(_symbol, interval, period.ToString()), Times.Exactly(2));
            _mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(_symbol), Times.Exactly(2));
            _mockBinanceClient.Verify(c => c.PlaceOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_symbol), Times.Exactly(2));
        }

        [TestMethod]
        public async Task TradeOnLimitAsync_WhenApiThrowsException_HandlesException()
        {
            // Arrange
            var interval = _dict[_symbol].Interval;
            var period = _dict[_symbol].Period;

            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_symbol, interval, period.ToString()))
                              .ReturnsAsync(new List<List<object>>());
            _mockBinanceClient.Setup(c => c.GetPriceBySymbolAsync(_symbol))
                              .ThrowsAsync(new Exception("API Error"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => _handler.TradeOnLimitAsync());
            _mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(_symbol), Times.Once);
        }

        [TestMethod]
        public async Task WaitBuyAsync_CompletesSuccessfully()
        {
            // Arrange
            var interval = _dict[_symbol].Interval;
            var period = _dict[_symbol].Period;
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var klines = Enumerable.Repeat(kline, period).ToList();

            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_symbol, interval, period.ToString()))
                              .ReturnsAsync(klines);
            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_symbol, interval, (period + 1).ToString()))
                              .ReturnsAsync(klines);

            var currencyForBuy = new Currency { Symbol = _symbol, Price = 90m };
            var currencyForSell = new Currency { Symbol = _symbol, Price = 100m };
            _mockBinanceClient.SetupSequence(c => c.GetPriceBySymbolAsync(_symbol))
                              .ReturnsAsync(currencyForBuy)
                              .ReturnsAsync(currencyForSell);

            _mockBinanceClient.Setup(c => c.PlaceOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                              .ReturnsAsync(string.Empty);

            var orders = new List<Order>() { new Order { Symbol = _symbol, Side = "BUY" } };
            var ordersEnd = new List<Order>();
            _mockBinanceClient.SetupSequence(c => c.GetOpenOrdersAsync(_symbol))
                              .ReturnsAsync(orders)
                              .ReturnsAsync(orders)
                              .ReturnsAsync(ordersEnd);

            // Act
            await _handler.WaitBuyAsync();

            // Assert
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_symbol), Times.Exactly(3));
        }

        [TestMethod]
        public async Task WaitSellAsync_CompletesSuccessfully()
        {
            // Arrange
            var interval = _dict[_symbol].Interval;
            var period = _dict[_symbol].Period;
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var klines = Enumerable.Repeat(kline, period).ToList();

            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_symbol, interval, period.ToString()))
                              .ReturnsAsync(klines);
            _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_symbol, interval, (period + 1).ToString()))
                              .ReturnsAsync(klines);

            var currencyForBuy = new Currency { Symbol = _symbol, Price = 90m };
            var currencyForSell = new Currency { Symbol = _symbol, Price = 100m };
            _mockBinanceClient.SetupSequence(c => c.GetPriceBySymbolAsync(_symbol))
                              .ReturnsAsync(currencyForBuy)
                              .ReturnsAsync(currencyForSell);

            _mockBinanceClient.Setup(c => c.PlaceOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                              .ReturnsAsync(string.Empty);

            var orders = new List<Order>() { new Order { Symbol = _symbol, Side = "SELL" } };
            var ordersEnd = new List<Order>();
            _mockBinanceClient.SetupSequence(c => c.GetOpenOrdersAsync(_symbol))
                              .ReturnsAsync(orders)
                              .ReturnsAsync(orders)
                              .ReturnsAsync(ordersEnd);

            // Act
            await _handler.WaitSellAsync();

            // Assert
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_symbol), Times.Exactly(3));
        }
    }
}