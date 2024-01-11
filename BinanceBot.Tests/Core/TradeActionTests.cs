using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using BinanceBot.Strategy;
using Moq;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class TradeActionTests
    {
        private readonly Mock<IBinanceClient> _mockBinanceClientTest = new();
        private readonly Mock<IBinanceClient> _mockBinanceClient = new();
        private readonly Mock<IPriceRetriever> _mockPriceRetriever = new();
        private readonly Mock<IVolatilityStrategy> _mockVolatilityStrategy = new();
        private readonly Mock<ILogger> _mockLogger = new();
        private readonly TradeAction _tradeAction;
        private readonly TradingStrategy _tradingStrategy = new() { TestMode = false };
        private readonly TradingStrategy _tradingStrategyTest = new();

        public TradeActionTests()
        {
            _tradeAction = new TradeAction(_mockBinanceClient.Object, _mockVolatilityStrategy.Object, _mockPriceRetriever.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task WaitBuyAsyncCompletesSuccessfullyTestStrategy()
        {
            // Arrange
            var orders = new List<Order>() { new Order { Symbol = _tradingStrategyTest.Symbol, Side = "BUY" } };
            var ordersEnd = new List<Order>();
            _mockBinanceClient.SetupSequence(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol))
                              .ReturnsAsync(orders)
                              .ReturnsAsync(orders)
                              .ReturnsAsync(ordersEnd);

            // Act
            await _tradeAction.WaitBuyAsync(_tradingStrategyTest.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol), Times.Exactly(3));
        }

        [TestMethod]
        public async Task WaitSellAsyncCompletesSuccessfullyTestStrategy()
        {
            // Arrange
            var orders = new List<Order>() { new Order { Symbol = _tradingStrategyTest.Symbol, Side = "SELL" } };
            var ordersEnd = new List<Order>();
            _mockBinanceClient.SetupSequence(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol))
                              .ReturnsAsync(orders)
                              .ReturnsAsync(orders)
                              .ReturnsAsync(ordersEnd);

            // Act
            await _tradeAction.WaitSellAsync(_tradingStrategyTest.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol), Times.Exactly(3));
        }

        [TestMethod]
        public async Task BuyValidParametersCallsBinanceClient()
        {
            // Arrange
            decimal currentCurrencyPrice = 100m;
            decimal volatility = 0.05m;

            _mockBinanceClient.Setup(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "BUY")).ReturnsAsync(It.IsAny<Order>());
            _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol)).ReturnsAsync(new List<Order>());

            // Act
            await _tradeAction.Buy(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "BUY"), Times.Once);
            _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(2));
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[ACHAT]"))), Times.AtLeastOnce);
            Assert.IsTrue(_tradingStrategy.OpenPosition);
        }

        [TestMethod]
        public async Task SellValidParametersCallsBinanceClient()
        {
            // Arrange
            var stopLossStrategy = new StopLossStrategy();
            decimal currentCurrencyPrice = 100m;
            decimal volatility = 0.05m;

            _mockVolatilityStrategy.Setup(c => c.DetermineLossStrategy(_tradingStrategy.CryptoPurchasePrice, volatility)).Returns(10000);
            _mockBinanceClient.Setup(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "SELL")).ReturnsAsync(It.IsAny<Order>());
            _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol)).ReturnsAsync(new List<Order>());

            // Act
            var (prixVenteCible, maxBenefitDone) = await _tradeAction.Sell(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "SELL"), Times.Once);
            _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(4));
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("STOPP LOSS"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("null"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[VENTE]"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("BENEFICE LIMITE"))), Times.AtLeastOnce);
            Assert.IsTrue(!_tradingStrategy.OpenPosition);
        }

        [TestMethod]
        public async Task BuyValidParametersCallsBinanceClientTestStrategy()
        {
            // Arrange
            decimal currentCurrencyPrice = 100m;
            decimal volatility = 0.05m;

            _mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(_tradingStrategyTest.Symbol, _tradingStrategyTest.Quantity, currentCurrencyPrice, "BUY")).ReturnsAsync(It.IsAny<TestOrder>());
            _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol)).ReturnsAsync(new List<Order>());

            // Act
            await _tradeAction.Buy(_tradingStrategyTest, currentCurrencyPrice, volatility, _tradingStrategyTest.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(_tradingStrategyTest.Symbol, _tradingStrategyTest.Quantity, currentCurrencyPrice, "BUY"), Times.Once);
            _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(2));
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[ACHAT]"))), Times.AtLeastOnce);
            Assert.IsTrue(_tradingStrategyTest.OpenPosition);
        }

        [TestMethod]
        public async Task SellValidParametersCallsBinanceClientTestStrategy()
        {
            // Arrange
            var stopLossStrategy = new StopLossStrategy();
            decimal currentCurrencyPrice = 100m;
            decimal volatility = 0.05m;

            _mockVolatilityStrategy.Setup(c => c.DetermineLossStrategy(_tradingStrategyTest.CryptoPurchasePrice, volatility)).Returns(10000);
            _mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(_tradingStrategyTest.Symbol, _tradingStrategyTest.Quantity, currentCurrencyPrice, "SELL")).ReturnsAsync(It.IsAny<TestOrder>());
            _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol)).ReturnsAsync(new List<Order>());

            // Act
            var (prixVenteCible, maxBenefitDone) = await _tradeAction.Sell(_tradingStrategyTest, currentCurrencyPrice, volatility, _tradingStrategyTest.Symbol);

            // Assert
            _mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(_tradingStrategyTest.Symbol, _tradingStrategyTest.Quantity, currentCurrencyPrice, "SELL"), Times.Once);
            _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(4));
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("STOPP LOSS"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("null"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[VENTE]"))), Times.AtLeastOnce);
            _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("BENEFICE LIMITE"))), Times.AtLeastOnce);
            Assert.IsTrue(!_tradingStrategyTest.OpenPosition);
        }
    }
}