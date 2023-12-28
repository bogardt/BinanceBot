using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System.Net;

namespace BinanceBot.Tests.Core
{
    [TestClass]
    public class BinanceClientTests
    {
        private readonly Mock<IBinanceClient> _mockClient;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<IHttpClientWrapper> _mockHttpClientWrapper;
        private readonly BinanceClient _binanceClient;
        private readonly IConfiguration _config;

        public BinanceClientTests()
        {
            var appSettings = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("AppSettings:Binance:ApiKey", "***"),
                new KeyValuePair<string, string?>("AppSettings:Binance:ApiSecret", "***")
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();
            _mockClient = new Mock<IBinanceClient>();
            _mockLogger = new Mock<ILogger>();
            _mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
            _binanceClient = new BinanceClient(_mockHttpClientWrapper.Object, _mockLogger.Object, _config);
        }

        [TestMethod]
        public async Task GetKLinesBySymbolMockedAsyncTest()
        {
            // Arrange
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

            // Act
            var klines = await _mockClient.Object.GetKLinesBySymbolAsync("BTCUSDT", "1m", "1");

            // Assert
            Assert.IsTrue(klines.Count == 1);
            Assert.IsTrue(klines[0].Count == 12);
        }

        [TestMethod]
        public async Task GetKLinesBySymbolAsync_ReturnsKLinesData()
        {
            // Arrange
            var expectedJsonResponse = "[[1703743320000,\"105.07000000\",\"105.34000000\",\"105.07000000\",\"105.29000000\",\"3911.95000000\",1703743379999,\"411810.32790000\",355,\"2528.48000000\",\"266172.60680000\",\"0\"],[1703743380000,\"105.30000000\",\"105.50000000\",\"105.26000000\",\"105.30000000\",\"3123.64000000\",1703743439999,\"329177.12570000\",388,\"1588.03000000\",\"167320.20810000\",\"0\"],[1703743440000,\"105.31000000\",\"105.36000000\",\"105.18000000\",\"105.34000000\",\"1697.03000000\",1703743499999,\"178640.07650000\",207,\"1418.11000000\",\"149285.24500000\",\"0\"],[1703743500000,\"105.35000000\",\"105.59000000\",\"105.32000000\",\"105.54000000\",\"5564.70000000\",1703743559999,\"587038.22590000\",643,\"3737.70000000\",\"394318.36610000\",\"0\"],[1703743560000,\"105.53000000\",\"105.63000000\",\"105.40000000\",\"105.42000000\",\"3281.83000000\",1703743619999,\"346349.35450000\",445,\"1550.78000000\",\"163670.19370000\",\"0\"],[1703743620000,\"105.43000000\",\"105.61000000\",\"105.39000000\",\"105.53000000\",\"3284.39000000\",1703743679999,\"346399.67380000\",323,\"1307.55000000\",\"137890.72440000\",\"0\"],[1703743680000,\"105.54000000\",\"105.67000000\",\"105.48000000\",\"105.57000000\",\"3072.70000000\",1703743739999,\"324419.64350000\",410,\"1615.12000000\",\"170538.58850000\",\"0\"],[1703743740000,\"105.57000000\",\"105.62000000\",\"105.49000000\",\"105.60000000\",\"3527.71000000\",1703743799999,\"372374.52950000\",334,\"1806.79000000\",\"190704.32620000\",\"0\"],[1703743800000,\"105.60000000\",\"105.61000000\",\"105.43000000\",\"105.46000000\",\"3999.19000000\",1703743859999,\"421970.87660000\",360,\"2426.25000000\",\"255985.65010000\",\"0\"],[1703743860000,\"105.47000000\",\"105.52000000\",\"105.46000000\",\"105.51000000\",\"96.99000000\",1703743919999,\"10230.16830000\",22,\"62.92000000\",\"6636.47540000\",\"0\"]]\r\n";
            _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedJsonResponse);

            // Act
            var result = await _binanceClient.GetKLinesBySymbolAsync("BTCUSDT", "1m", "10");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<List<object>>));
            Assert.AreEqual(10, result.Count);
        }

        [TestMethod]
        public async Task GetKLinesBySymbolAsync_ReturnsKLinesDataEmpty()
        {
            // Arrange
            var expectedJsonResponse = "[]\r\n";
            _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedJsonResponse);

            // Act
            var result = await _binanceClient.GetKLinesBySymbolAsync("BTCUSDT", "1m", "10");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<List<object>>));
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetPriceBySymbolMockAsyncTest()
        {
            // Arrange
            _mockClient.Setup(client => client.GetPriceBySymbolAsync(It.IsAny<string>())).ReturnsAsync(new Currency
            {
                Symbol = "BTCUSDT",
                Price = 50000,
            });

            // Act
            var currency = await _mockClient.Object.GetPriceBySymbolAsync("BTCUSDT");

            // Assert
            Assert.IsTrue(!string.IsNullOrEmpty(currency.Symbol));
            Assert.IsTrue(currency.Price > 0);
        }

        [TestMethod]
        public async Task GetPriceBySymbolMockAsync_ReturnsPriceSuccessfully()
        {
            // Arrange
            var expectedJsonResponse = "{\"price\":\"50000.00\"}";
            _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedJsonResponse);

            // Act
            var result = await _binanceClient.GetPriceBySymbolAsync("BTCUSDT");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(50000.00m, result.Price);
        }

        [TestMethod]
        public async Task GetPriceBySymbolMockAsync_HandlesMalformedResponse()
        {
            // Arrange
            var malformedJsonResponse = "error";
            _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(malformedJsonResponse);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<JsonReaderException>(
                () => _binanceClient.GetPriceBySymbolAsync("BTCUSDT")
            );
        }

        [TestMethod]
        public async Task GetPriceBySymbolMockAsync_HandlesConnectionFailure()
        {
            // Arrange
            _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
                .ThrowsAsync(new HttpRequestException("Connection failure"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => _binanceClient.GetPriceBySymbolAsync("BTCUSDT")
            );
        }

        [TestMethod]
        public async Task GetPriceBySymbolMockAsync_HandlesInvalidSymbol()
        {
            // Arrange
            var invalidSymbolResponse = "{\"code\":-1121,\"msg\":\"Invalid symbol.\"}";
            _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(invalidSymbolResponse);

            // Act
            var result = await _binanceClient.GetPriceBySymbolAsync("BTCUSDT_FAKE_SYMBOL");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Invalid symbol.", result.Msg);
            Assert.AreEqual(-1121, result.Code);
        }

        [TestMethod]
        public async Task GetOpenOrdersMockAsyncTest()
        {
            // Arrange
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

            // Act
            var orders = await _mockClient.Object.GetOpenOrdersAsync("BTCUSDT");

            // Assert
            Assert.IsNotNull(orders);
            Assert.IsTrue(orders.Count == 1);
        }

        [TestMethod]
        public async Task GetOpenOrdersAsync_ReturnsOpenOrdersSuccessfully()
        {
            // Arrange
            var expectedJsonResponse = "[{\"orderId\": 1, \"symbol\": \"BTCUSDT\"}, {\"orderId\": 2, \"symbol\": \"ETHUSDT\"}]";
            var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedJsonResponse)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeResponseMessage);

            // Act
            var result = await _binanceClient.GetOpenOrdersAsync("BTCUSDT");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task GetOpenOrdersAsync_HandlesMalformedResponse()
        {
            // Arrange
            var malformedJsonResponse = "error";
            var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(malformedJsonResponse)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeResponseMessage);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<JsonReaderException>(
                () => _binanceClient.GetOpenOrdersAsync("BTCUSDT")
            );
        }

        [TestMethod]
        public async Task GetOpenOrdersAsync_HandlesConnectionFailure()
        {
            // Arrange
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Connection failure"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => _binanceClient.GetOpenOrdersAsync("BTCUSDT")
            );
        }

        [TestMethod]
        public async Task GetOpenOrdersAsync_HandlesNoOpenOrders()
        {
            // Arrange
            var emptyJsonResponse = "[]";
            var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(emptyJsonResponse)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeResponseMessage);

            // Act
            var result = await _binanceClient.GetOpenOrdersAsync("BTCUSDT");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task PlaceOrderMockAsyncTest()
        {
            _mockClient.Setup(client => client.PlaceOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>())).ReturnsAsync("123456789");

            var order = await _mockClient.Object.PlaceOrderAsync("BTCUSDT", 0.0001m, 50000, "BUY");
            Assert.IsTrue(!string.IsNullOrEmpty(order));
        }

        [TestMethod]
        public async Task PlaceOrderAsync_SuccessfullyPlacesOrder()
        {
            // Arrange
            var expectedJsonResponse = "{\"orderId\": 12345, \"status\": \"SUCCESS\"}";
            var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedJsonResponse)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeResponseMessage);

            // Act
            var result = await _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("\"orderId\": 12345"));
        }

        [TestMethod]
        public async Task PlaceOrderAsync_HandlesMalformedResponse()
        {
            // Arrange
            var malformedJsonResponse = "{\"unexpectedField\":\"value\"}";
            var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(malformedJsonResponse)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeResponseMessage);

            // Act & Assert
            var result = await _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY");
            Assert.IsFalse(result.Contains("\"orderId\":"));
        }

        [TestMethod]
        public async Task PlaceOrderAsync_HandlesConnectionFailure()
        {
            // Arrange
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Connection failure"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY")
            );
        }

        [TestMethod]
        public async Task PlaceOrderAsync_HandlesInvalidParameters()
        {
            // Arrange
            var errorResponse = "{\"code\": -1100, \"msg\": \"Illegal characters found in parameter 'price'; legal range is '^([0-9]{1,20})(\\.[0-9]{1,8})?$'.\"}";
            var fakeErrorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorResponse)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeErrorResponse);

            // Act
            var result = await _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, -50000.0m, "BUY");

            // Assert
            Assert.IsTrue(result.Contains("\"code\": -1100"));
        }
    }
}