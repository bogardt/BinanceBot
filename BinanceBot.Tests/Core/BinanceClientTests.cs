using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Security.Principal;

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

        //[TestMethod]
        //public void CheckLoggerCallOnConstructorCall()
        //{
        //    _mockLogger.Verify(logger => logger.WriteLog(It.IsAny<string>()), Times.Once);
        //}

        [TestMethod]
        public async Task GetKLinesBySymbolAsyncReturnsKLinesData()
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
        public async Task GetKLinesBySymbolAsyncReturnsKLinesDataEmpty()
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
        public async Task GetPriceBySymbolMockAsyncReturnsPriceSuccessfully()
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
        public async Task GetPriceBySymbolMockAsyncHandlesMalformedResponse()
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
        public async Task GetPriceBySymbolMockAsyncHandlesNullResponse()
        {
            // Arrange
            _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
                .ReturnsAsync(string.Empty);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<JsonReaderException>(
                () => _binanceClient.GetPriceBySymbolAsync("BTCUSDT")
            );
        }

        [TestMethod]
        public async Task GetPriceBySymbolMockAsyncHandlesConnectionFailure()
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
        public async Task GetPriceBySymbolMockAsyncHandlesInvalidSymbol()
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
        public async Task GetOpenOrdersAsyncReturnsOpenOrdersSuccessfully()
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
        public async Task GetOpenOrdersAsyncHandlesMalformedResponse()
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
        public async Task GetOpenOrdersAsyncHandlesNullResponse()
        {
            // Arrange
            var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeResponseMessage);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<JsonReaderException>(
                () => _binanceClient.GetOpenOrdersAsync("BTCUSDT")
            );
        }

        [TestMethod]
        public async Task GetOpenOrdersAsyncHandlesConnectionFailure()
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
        public async Task GetOpenOrdersAsyncHandlesNoOpenOrders()
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
        public async Task PlaceTestOrderAsyncSuccessfullyPlacesOrder()
        {
            // Arrange
            var testOrder = new TestOrder
            {
                StandardCommissionForOrder = new CommissionRates
                {
                    Maker = "0.001",
                    Taker = "0.001"
                },
                TaxCommissionForOrder = new CommissionRates
                {
                    Maker = "0.000",
                    Taker = "0.000"
                },
                Discount = new Discount
                {
                    DiscountValue = "0.75",
                    EnabledForAccount = false,
                    EnabledForSymbol = false
                }
            };
            var serializedTestOrder = JsonConvert.SerializeObject(testOrder);
            var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(serializedTestOrder)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeResponseMessage);

            // Act
            var result = await _binanceClient.PlaceTestOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StandardCommissionForOrder.Maker == "0.001");
        }

        [TestMethod]
        public async Task PlaceTestOrderAsyncHandlesMalformedResponse()
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
            var result = await _binanceClient.PlaceTestOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY");
            Assert.IsNull(result.Discount);
            Assert.IsNull(result.TaxCommissionForOrder);
            Assert.IsNull(result.StandardCommissionForOrder);
            Assert.IsNull(result.Code);
            Assert.IsNull(result.Message);
        }

        [TestMethod]
        public async Task PlaceTestOrderAsyncHandlesConnectionFailure()
        {
            // Arrange
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Connection failure"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<HttpRequestException>(
                () => _binanceClient.PlaceTestOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY")
            );
        }

        [TestMethod]
        public async Task PlaceTestOrderAsyncHandlesInvalidParameters()
        {
            // Arrange
            var errorResponse = "{\"code\": -1100, \"msg\": \"Illegal characters found in parameter 'price'\"}";
            var fakeErrorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorResponse)
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeErrorResponse);

            // Act
            var result = await _binanceClient.PlaceTestOrderAsync("BTCUSDT", 1.0m, -50000.0m, "BUY");

            // Assert
            Assert.IsTrue(result.Code == -1100);
            Assert.IsTrue(result?.Message?.Contains("Illegal characters found in parameter 'price'"));
        }

        [TestMethod]
        public async Task GetAccountInfosAsyncReturnsAccount()
        {
            // construit moi un objet account entierement
            // Arrange
            var account = new Account
            {
                Balances = new[]
                {
                    new Balance
                    {
                        Asset = "BNB",
                        Free = "1"
                    },
                    new Balance
                    {
                        Asset = "USDT",
                        Free = "1"
                    },
                    new Balance
                    {
                        Asset = "SOL",
                        Free = "1"
                    },
                }
            };
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(account))
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(response);

            // Act
            var result = await _binanceClient.GetAccountInfosAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Balances.Length);
            Assert.AreEqual("BNB", result.Balances[0].Asset);
            Assert.AreEqual("1", result.Balances[0].Free);
            Assert.AreEqual("USDT", result.Balances[1].Asset);
            Assert.AreEqual("1", result.Balances[1].Free);
            Assert.AreEqual("SOL", result.Balances[2].Asset);
            Assert.AreEqual("1", result.Balances[2].Free);
        }

        [TestMethod]
        public async Task GetCommissionBySymbolAsyncReturnsCommission()
        {
            // Arrange
            var commission = new Commission
            {
                StandardCommission = new CommissionRates
                {
                    Maker = "0.001",
                    Taker = "0.001"
                },
                TaxCommission = new CommissionRates
                {
                    Maker = "0.000",
                    Taker = "0.000"
                },
                Discount = new Discount
                {
                    DiscountValue = "0.75",
                    EnabledForAccount = false,
                    EnabledForSymbol = false
                }
            };
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonConvert.SerializeObject(commission))
            };
            _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(response);

            string symbol = "SOLUSDT";

            // Act
            var result = await _binanceClient.GetCommissionBySymbolAsync(symbol);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("0.001", result.StandardCommission.Maker);
            Assert.AreEqual("0.001", result.StandardCommission.Taker);
            Assert.AreEqual("0.000", result.TaxCommission.Maker);
            Assert.AreEqual("0.000", result.TaxCommission.Taker);
            Assert.AreEqual("0.75", result.Discount.DiscountValue);
            Assert.IsFalse(result.Discount.EnabledForAccount);
            Assert.IsFalse(result.Discount.EnabledForSymbol);
        }
    }
}