using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class TradeActionTests
    {
        private readonly Mock<IBinanceClient> _mockBinanceClient = new();
        private readonly Mock<ILogger> _mockLogger = new();
        private readonly Mock<IVolatilityStrategy> _mockVolatilityStrategy = new();
        private readonly TradeAction _tradeAction;
        public TradeActionTests()
        {
            _tradeAction = new TradeAction(_mockBinanceClient.Object, _mockVolatilityStrategy.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task WaitBuyAsyncCompletesSuccessfully()
        {
            // Arrange
            var orders = new List<Order>() { new Order { Symbol = TradeSetup.Symbol, Side = "BUY" } };
            var ordersEnd = new List<Order>();
            _mockBinanceClient.SetupSequence(c => c.GetOpenOrdersAsync(TradeSetup.Symbol))
                              .ReturnsAsync(orders)
                              .ReturnsAsync(orders)
                              .ReturnsAsync(ordersEnd);

            // Act
            await _tradeAction.WaitBuyAsync(TradeSetup.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(TradeSetup.Symbol), Times.Exactly(3));
        }

        [TestMethod]
        public async Task WaitSellAsyncCompletesSuccessfully()
        {
            // Arrange
            var orders = new List<Order>() { new Order { Symbol = TradeSetup.Symbol, Side = "SELL" } };
            var ordersEnd = new List<Order>();
            _mockBinanceClient.SetupSequence(c => c.GetOpenOrdersAsync(TradeSetup.Symbol))
                              .ReturnsAsync(orders)
                              .ReturnsAsync(orders)
                              .ReturnsAsync(ordersEnd);

            // Act
            await _tradeAction.WaitSellAsync(TradeSetup.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(TradeSetup.Symbol), Times.Exactly(3));
        }

        [TestMethod]
        public async Task BuyValidParametersCallsBinanceClient()
        {
            // Arrange
            var tradingConfig = new TradingConfig(TradeSetup.Dict, TradeSetup.Symbol);
            decimal currentCurrencyPrice = 100m;
            decimal volatility = 0.05m;

            _mockBinanceClient.Setup(c => c.PlaceOrderAsync(TradeSetup.Symbol, tradingConfig.Quantity, currentCurrencyPrice, "BUY")).ReturnsAsync("OK");
            _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(TradeSetup.Symbol)).ReturnsAsync(new List<Order>());

            // Act
            await _tradeAction.Buy(tradingConfig, currentCurrencyPrice, volatility, TradeSetup.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.PlaceOrderAsync(TradeSetup.Symbol, tradingConfig.Quantity, currentCurrencyPrice, "BUY"), Times.Once);
            _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(2));
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[ACHAT]"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("OK"))), Times.AtLeastOnce);
            Assert.IsTrue(tradingConfig.OpenPosition);
        }

        [TestMethod]
        public async Task SellValidParametersCallsBinanceClient()
        {
            // Arrange
            var tradingConfig = new TradingConfig(TradeSetup.Dict, TradeSetup.Symbol);
            decimal currentCurrencyPrice = 100m;
            decimal volatility = 0.05m;

            _mockVolatilityStrategy.Setup(c => c.DetermineLossStrategy(volatility, tradingConfig)).Returns(10000);
            _mockBinanceClient.Setup(c => c.PlaceOrderAsync(TradeSetup.Symbol, tradingConfig.Quantity, currentCurrencyPrice, "SELL")).ReturnsAsync("OK");
            _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(TradeSetup.Symbol)).ReturnsAsync(new List<Order>());

            // Act
            var (prixVenteCible, maxBenefitDone) = await _tradeAction.Sell(tradingConfig, currentCurrencyPrice, volatility, TradeSetup.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.PlaceOrderAsync(TradeSetup.Symbol, tradingConfig.Quantity, currentCurrencyPrice, "SELL"), Times.Once);
            _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(4));
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("STOPP LOSS"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[VENTE]"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("OK"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("BENEFICE LIMITE"))), Times.AtLeastOnce);
            Assert.IsTrue(!tradingConfig.OpenPosition);
        }
    }
}