using BinanceBot.Abstraction;
using BinanceBot.BinanceApi;
using BinanceBot.BinanceApi.Model;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using System.Net;
using System.Security.Principal;

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
        _mockHttpClientWrapper.Setup(client => client.SendStringAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await response.Content.ReadAsStringAsync());
        _mockApiValidatorService.Setup(x => x.ValidateAsync<Account>(It.IsAny<string>()))
            .ReturnsAsync(account);

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
    public async Task GetPriceBySymbolAsync_ReturnsCurrencyPrice()
    {
        // Arrange
        var fakeCurrency = new Currency { /* Initialiser avec des valeurs test */ };
        var fakeResponse = JsonConvert.SerializeObject(fakeCurrency);

        _mockHttpClientWrapper.Setup(x => x.GetStringAsync(It.IsAny<string>()))
                              .ReturnsAsync(fakeResponse);
        _mockApiValidatorService.Setup(v => v.ValidateAsync<Currency>(It.IsAny<string>()))
                         .ReturnsAsync(fakeCurrency);

        // Act
        var currency = await _binanceClient.GetPriceBySymbolAsync("BTCUSDT");

        // Assert
        Assert.IsNotNull(currency);
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
        _mockHttpClientWrapper.Setup(client => client.SendStringAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await response.Content.ReadAsStringAsync());
        _mockApiValidatorService.Setup(x => x.ValidateAsync<Commission>(It.IsAny<string>()))
            .ReturnsAsync(commission);

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
        _mockHttpClientWrapper.Setup(client => client.SendStringAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await fakeResponseMessage.Content.ReadAsStringAsync());
        //_mockApiValidatorService.Setup(x => x.ValidateAsync<List<Order>>(It.IsAny<string>()))
        //   .ReturnsAsync(order);
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
        _mockHttpClientWrapper.Setup(client => client.SendStringAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await fakeResponseMessage.Content.ReadAsStringAsync());
        _mockApiValidatorService.Setup(x => x.ValidateAsync<TestOrder>(It.IsAny<string>()))
            .ReturnsAsync(testOrder);

        // Act
        var result = await _binanceClient.PlaceTestOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.StandardCommissionForOrder.Maker == "0.001");
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
        _mockHttpClientWrapper.Setup(client => client.SendStringAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(await fakeResponseMessage.Content.ReadAsStringAsync());
        _mockApiValidatorService.Setup(x => x.ValidateAsync<Order>(It.IsAny<string>()))
            .ReturnsAsync(order);

        // Act
        var result = await _binanceClient.PlaceOrderAsync("BTCUSDT", 1.0m, 50000.0m, "BUY");

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.OrderId == 1);
        Assert.IsTrue(result.Symbol == "BTCUSDT");
        Assert.IsTrue(result.Side == "BUY");
    }
}