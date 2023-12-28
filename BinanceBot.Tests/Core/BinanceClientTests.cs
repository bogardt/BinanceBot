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
        private readonly Mock<ILogger> _mockLogger = new();
        private readonly Mock<IHttpClientWrapper> _mockHttpClientWrapper = new();
        private readonly Mock<IConfiguration> _mockConfig = new();
        private readonly BinanceClient _binanceClient;

        public BinanceClientTests()
        {
            _binanceClient = new BinanceClient(_mockHttpClientWrapper.Object, _mockLogger.Object, _mockConfig.Object);
        }

        [TestMethod]
        public async Task GetKLinesBySymbolAsync_ReturnsKLinesData()
        {
            // Arrange
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var klines = Enumerable.Repeat(kline, 10).ToList();
            
            _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(JsonConvert.SerializeObject(klines));

            // Act
            var result = await _binanceClient.GetKLinesBySymbolAsync("BTCUSDT", "1m", "10");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<List<object>>));
            Assert.AreEqual(10, result.Count);
            Assert.IsTrue(klines[0].Count == 12);
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