using BinanceBot.Abstraction;
using BinanceBot.BinanceApi;
using BinanceBot.BinanceApi.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System.Net;

namespace BinanceBot.Tests.BinanceApi;

[TestClass]
public class BinanceClientTests
{
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly Mock<IHttpClientWrapper> _mockHttpClientWrapper = new();
    private readonly Mock<IApiValidatorService> _mockApiValidatorService = new();
    private readonly Mock<IConfiguration> _mockConfig = new();
    private readonly BinanceClient _binanceClient;

    public BinanceClientTests() => _binanceClient = new BinanceClient(_mockApiValidatorService.Object, _mockHttpClientWrapper.Object, _mockConfig.Object);

    //[TestMethod]
    //public void CheckLoggerCallOnConstructorCall()
    //{
    //    _mockLogger.Verify(logger => logger.WriteLog(It.IsAny<string>()), Times.Once);
    //}

    [TestMethod]
    public async Task GetAccountInfosAsyncReturnsAccount()
    {
        // Arrange
        var account = new Account
        {
            Balances =
            [
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
            ]
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
    public async Task GetAccountInfosAsyncReturnsAccountThrow()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(string.Empty)
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetAccountInfosAsync()
        );
    }

    [TestMethod]
    public async Task GetAccountInfosAsyncReturnsAccountThrowAccountEmpty()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(null))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetAccountInfosAsync()
        );
    }

    [TestMethod]
    public async Task GetAccountInfosAsyncHandlesConnectionFailure()
    {
        // Arrange
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection failure"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(
            () => _binanceClient.GetAccountInfosAsync()
        );
    }

    [TestMethod]
    public async Task GetAccountInfosAsyncReturnsAccountThrowExceptionErrorDetected()
    {
        // Arrange
        var account = new Account
        {
            Code = 100,
            Message = "Something"
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(account))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(response);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _binanceClient.GetAccountInfosAsync()
        );
        Assert.IsTrue(exception.Message.Contains("Unable to get account infos "));
    }


    [TestMethod]
    public async Task GetCommissionBySymbolAsyncReturnsCommission()
    {
        // Arrange
        var commission = new Commission
        {
            StandardCommission = new CommissionRate
            {
                Maker = "0.001",
                Taker = "0.001"
            },
            TaxCommission = new CommissionRate
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


    [TestMethod]
    public async Task GetCommissionBySymbolAsyncReturnsAccountThrow()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(string.Empty)
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetCommissionBySymbolAsync("BTCUSDT")
        );
    }

    [TestMethod]
    public async Task GetCommissionBySymbolAsyncReturnsAccountThrowAccountEmpty()
    {
        // Arrange
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(null))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetCommissionBySymbolAsync("BTCUSDT")
        );
    }

    [TestMethod]
    public async Task GetCommissionBySymbolAsyncHandlesConnectionFailure()
    {
        // Arrange
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection failure"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(
            () => _binanceClient.GetCommissionBySymbolAsync("BTCUSDT")
        );
    }

    [TestMethod]
    public async Task GetCommissionBySymbolAsyncReturnsAccountThrowExceptionErrorDetected()
    {
        // Arrange
        var commission = new Commission
        {
            Code = 100,
            Message = "Something"
        };

        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonConvert.SerializeObject(commission))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(response);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _binanceClient.GetCommissionBySymbolAsync("BTCUSDT")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to get wallet for "));
    }

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
        Assert.IsInstanceOfType(result, typeof(IEnumerable<IEnumerable<object>>));
        Assert.AreEqual(10, result.Count());
        Assert.IsTrue(klines[0].Count == 12);
    }

    [TestMethod]
    public async Task GetKLinesBySymbolAsyncReturnsKLinesDataEmpty()
    {
        // Arrange
        _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(JsonConvert.SerializeObject(new List<object>()));

        // Act
        var result = await _binanceClient.GetKLinesBySymbolAsync("BTCUSDT", "1m", "10");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(IEnumerable<IEnumerable<object>>));
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task GetKLinesBySymbolAsyncThrowOnMalformedResponse()
    {
        // Arrange
        _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(string.Empty);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetKLinesBySymbolAsync("BTCUSDT", "1m", "10")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to get klines for "));
    }

    [TestMethod]
    public async Task GetKLinesBySymbolAsyncThrowOnNullResponse()
    {
        // Arrange
        _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(JsonConvert.SerializeObject(null));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetKLinesBySymbolAsync("BTCUSDT", "1m", "10")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to get klines for "));
    }

    [TestMethod]
    public async Task GetKLinesBySymbolAsyncHandlesConnectionFailure()
    {
        // Arrange
        _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException("Connection failure"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<HttpRequestException>(
            () => _binanceClient.GetKLinesBySymbolAsync("BTCUSDT", "1m", "10")
        );
    }

    [TestMethod]
    public async Task GetPriceBySymbolMockAsyncReturnsPriceSuccessfully()
    {
        // Arrange
        var currency = new Currency
        {
            Price = 50000.00m
        };
        _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(JsonConvert.SerializeObject(currency));

        // Act
        var result = await _binanceClient.GetPriceBySymbolAsync("BTCUSDT");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(50000.00m, result.Price);
    }

    [TestMethod]
    public async Task GetPriceBySymbolAsyncHandlesMalformedResponse()
    {
        // Arrange
        _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(string.Empty);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetPriceBySymbolAsync("BTCUSDT")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to get price for "));
    }

    [TestMethod]
    public async Task GetPriceBySymbolAsyncHandlesNullResponse()
    {
        // Arrange
        _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(JsonConvert.SerializeObject(null));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetPriceBySymbolAsync("BTCUSDT")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to get price for "));
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
        var errorResponse = new Currency
        {
            Code = -1121,
            Message = "Invalid symbol."
        };
        _mockHttpClientWrapper.Setup(client => client.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync(JsonConvert.SerializeObject(errorResponse));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _binanceClient.GetPriceBySymbolAsync("BTCUSDT")
        );

        Assert.IsTrue(exception.Message.Contains("Unable to get price for "));
    }

    [TestMethod]
    public async Task GetOpenOrdersAsyncReturnsOpenOrdersSuccessfully()
    {
        // Arrange
        var order = new List<Order>
        {
            new() {
                OrderId = 1,
                Symbol = "BTCUSDT"
            },
            new() {
                OrderId = 2,
                Symbol = "ETHUSDT"
            }
        };
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(order))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act
        var result = await _binanceClient.GetOpenOrdersAsync("BTCUSDT");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count() == 2);
        Assert.IsTrue(result.ElementAt(0).OrderId == 1);
        Assert.IsTrue(result.ElementAt(0).Symbol == "BTCUSDT");
        Assert.IsTrue(result.ElementAt(1).OrderId == 2);
        Assert.IsTrue(result.ElementAt(1).Symbol == "ETHUSDT");
    }

    [TestMethod]
    public async Task GetOpenOrdersAsyncHandlesNoOpenOrders()
    {
        // Arrange
        var emptyJsonResponse = new List<Order>();
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(emptyJsonResponse))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act
        var result = await _binanceClient.GetOpenOrdersAsync("BTCUSDT");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task GetOpenOrdersAsyncHandlesMalformedResponse()
    {
        // Arrange
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty)
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetOpenOrdersAsync("BTCUSDT")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to get orders for "));
    }

    [TestMethod]
    public async Task GetOpenOrdersAsyncHandlesNullResponse()
    {
        // Arrange
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(null))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.GetOpenOrdersAsync("BTCUSDT")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to get orders for "));
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
    public async Task PlaceTestOrderAsyncSuccessfullyPlacesOrder()
    {
        // Arrange
        var testOrder = new TestOrder
        {
            StandardCommissionForOrder = new CommissionRate
            {
                Maker = "0.001",
                Taker = "0.001"
            },
            TaxCommissionForOrder = new CommissionRate
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
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty)
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.PlaceTestOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to place test order for "));
    }

    [TestMethod]
    public async Task PlaceTestOrderAsyncHandlesNullResponse()
    {
        // Arrange
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(null))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.PlaceTestOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to place test order for "));
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
        var errorResponse = new TestOrder
        {
            Code = -1100,
            Message = "Illegal characters found in parameter 'price'"
        };
        var fakeErrorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(JsonConvert.SerializeObject(errorResponse))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeErrorResponse);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _binanceClient.PlaceTestOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY")
        );
        Assert.IsTrue(exception.Message.Contains("Illegal characters found in parameter 'price'"));
    }

    [TestMethod]
    public async Task PlaceOrderAsyncSuccessfullyPlacesOrder()
    {
        // Arrange
        var order = new Order
        {
            OrderId = 1,
            Symbol = "BTCUSDT",
            Side = "BUY"
        };
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(order))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act
        var result = await _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.OrderId == 1);
        Assert.IsTrue(result.Symbol == "BTCUSDT");
        Assert.IsTrue(result.Side == "BUY");
    }

    [TestMethod]
    public async Task PlaceOrderAsyncHandlesMalformedResponse()
    {
        // Arrange
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(string.Empty)
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to place order for "));
    }

    [TestMethod]
    public async Task PlaceOrderAsyncHandlesNullResponse()
    {
        // Arrange
        var fakeResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(null))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeResponseMessage);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<JsonReaderException>(
            () => _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY")
        );
        Assert.IsTrue(exception.Message.Contains("Unable to place order for "));
    }

    [TestMethod]
    public async Task PlaceOrderAsyncHandlesConnectionFailure()
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
    public async Task PlaceOrderAsyncHandlesInvalidParameters()
    {
        // Arrange
        var errorResponse = new TestOrder
        {
            Code = -1100,
            Message = "Illegal characters found in parameter 'price'"
        };
        var fakeErrorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(JsonConvert.SerializeObject(errorResponse))
        };
        _mockHttpClientWrapper.Setup(client => client.SendAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(fakeErrorResponse);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<Exception>(
            () => _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY")
        );
        Assert.IsTrue(exception.Message.Contains("Illegal characters found in parameter 'price'"));
    }
}