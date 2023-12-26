using BinanceBot;
using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BinanceBot.Tests
{
    [TestClass]
    public class BinanceClientTests
    {
        private readonly Mock<IBinanceClient> _mockClient;
        private readonly Dictionary<string, string> _inMemorySettings = new()
        {
            { "ApiKey", "***" },
            { "ApiSecret", "***" },
        };
        private readonly IConfigurationRoot _configuration;
        private readonly BinanceClient _client;

        public BinanceClientTests()
        {
            _configuration = new ConfigurationBuilder()
                                     .AddInMemoryCollection(_inMemorySettings)
                                     .Build();

            _client = new BinanceClient(_configuration, testApi: true);
            _mockClient = new Mock<IBinanceClient>();
        }

        [TestMethod]
        public async Task GetKLinesBySymbolTest()
        {
            _mockClient.Setup(client => client.GetKLinesBySymbolAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<List<object>>
            {
                new List<object>
                {
                    1619458560000,
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
                    "0.00000000",
                },
            });

            var klines = await _mockClient.Object.GetKLinesBySymbolAsync("BTCUSDT", "1m", "1");
            Assert.IsTrue(klines.Count == 1);
            Assert.IsTrue(klines[0].Count == 12);
        }

        [TestMethod]
        public async Task GetKLinesBySymbolAsyncTest()
        {
            var klines = await _client.GetKLinesBySymbolAsync("BTCUSDT", "1m", "1");
            Assert.IsTrue(klines.Count == 1);
            Assert.IsTrue(klines[0].Count == 12);
        }

        [TestMethod]
        public async Task GetPriceBySymbolTest()
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
        public async Task GetPriceBySymbolAsyncTest()
        {
            var currency = await _client.GetPriceBySymbolAsync("BTCUSDT");
            Assert.IsTrue(!string.IsNullOrEmpty(currency.Symbol));
            Assert.IsTrue(currency.Price > 0);
        }

        [TestMethod]
        public async Task GetOpenOrdersAsyncTest()
        {
            var orders = await _client.GetOpenOrdersAsync("BTCUSDT");
            Assert.IsTrue(orders.Count == 1);
            Assert.IsTrue(orders[0].Symbol == "BTCUSDT");
        }

        [TestMethod]
        public async Task GetBNBBalanceTest()
        {
            var balance = await _client.GetBNBBalance();
            Assert.IsTrue(balance > 0);
        }

        [TestMethod]
        public async Task RecuperePrixRecentTest()
        {
            var prices = await _client.RecuperePrixRecent("BTCUSDT", "1m", 1);
            Assert.IsTrue(prices.Count == 1);
            Assert.IsTrue(prices[0] > 0);
        }
    }
}