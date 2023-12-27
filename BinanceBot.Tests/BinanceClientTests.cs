using BinanceBot.Abstraction;
using BinanceBot.Model;
using Moq;

namespace BinanceBot.Tests
{
    [TestClass]
    public class BinanceClientTests
    {
        private readonly Mock<IBinanceClient> _mockClient;

        public BinanceClientTests()
        {
            _mockClient = new Mock<IBinanceClient>();
        }

        [TestMethod]
        public async Task GetKLinesBySymbolMockedAsyncTest()
        {
            _mockClient.Setup(client => client.GetKLinesBySymbolAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<List<object>>
            {
                new List<object>
                {
                    16,
                    "0.00000000",
                    "0.00000000",
                    "0.00000000",
                    "0.00000000",
                    "0.00000000",
                    "0.00000000",
                    "0.00000000",
                    "0.00000000",
                    "0.00000000",
                    "0.00000000",
                    125.21m
                },
            });

            var klines = await _mockClient.Object.GetKLinesBySymbolAsync("BTCUSDT", "1m", "1");
            Assert.IsTrue(klines.Count == 1);
            Assert.IsTrue(klines[0].Count == 12);
        }

        [TestMethod]
        public async Task GetPriceBySymbolMockAsyncTest()
        {
            _mockClient.Setup(client => client.GetPriceBySymbolAsync(It.IsAny<string>())).ReturnsAsync(new Currency
            {
                Symbol = "BTCUSDT",
                Price = 50000,
            });
            var currency = await _mockClient.Object.GetPriceBySymbolAsync("BTCUSDT");
            Assert.IsTrue(!string.IsNullOrEmpty(currency.Symbol));
            Assert.IsTrue(currency.Price > 0);
        }

        [TestMethod]
        public async Task GetOpenOrdersMockAsyncTest()
        {
            _mockClient.Setup(client => client.GetOpenOrdersAsync(It.IsAny<string>())).ReturnsAsync(new List<Order>
            {
                new Order
                {
                    Symbol = "BTCUSDT",
                    Price = "50000",
                    Side = "BUY",
                    Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                },
            });

            var orders = await _mockClient.Object.GetOpenOrdersAsync("BTCUSDT");
            Assert.IsTrue(orders.Count == 1);
        }

        [TestMethod]
        public async Task PlaceOrderMockAsyncTest()
        {
            _mockClient.Setup(client => client.PlaceOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>())).ReturnsAsync("123456789");

            var order = await _mockClient.Object.PlaceOrderAsync("BTCUSDT", 0.0001m, 50000, "BUY");
            Assert.IsTrue(!string.IsNullOrEmpty(order));
        }
    }
}