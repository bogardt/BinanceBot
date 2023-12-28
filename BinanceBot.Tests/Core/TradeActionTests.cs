using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Reflection.Metadata;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class TradeActionTests
    {
        private readonly Mock<IBinanceClient> _mockBinanceClient = new();
        private readonly Mock<ILogger> _mockLogger = new();
        private readonly Mock<IVolatilityStrategy> _volatilityStrategy = new();
        private readonly TradeAction _tradeAction;
        public TradeActionTests()
        {
            _tradeAction = new TradeAction(_mockBinanceClient.Object, _volatilityStrategy.Object, _mockLogger.Object);
        }

        [TestMethod]
        public async Task WaitBuyAsync_CompletesSuccessfully()
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
        public async Task WaitSellAsync_CompletesSuccessfully()
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
    }
}