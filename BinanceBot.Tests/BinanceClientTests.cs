using BinanceBot;
using BinanceBot.BinanceApi;
using Microsoft.Extensions.Configuration;

namespace BinanceBot.Tests
{
    [TestClass]
    public class BinanceClientTests
    {
        private readonly Dictionary<string, string> _inMemorySettings = new()
        {
            { "", "" },
        };
        private readonly IConfigurationRoot _configuration;
        private readonly BinanceClient _client;

        public BinanceClientTests()
        {
            _configuration = new ConfigurationBuilder()
                                     .AddInMemoryCollection(_inMemorySettings)
                                     .Build();

            _client = new BinanceClient(_configuration, true);
        }

        [TestMethod]
        public async Task GetKLinesBySymbolAsyncTest()
        {
            var klines = await _client.GetKLinesBySymbolAsync("BTCUSDT", "1m", "1");
            Assert.IsTrue(klines.Count == 1);
            Assert.IsTrue(klines[0].Count == 12);
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