using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Strategy;
using Moq;

namespace BinanceBot.Tests.Core;

[TestClass]
public class PriceRetrieverTests
{
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly Mock<IBinanceClient> _mockBinanceClient = new();
    private readonly PriceRetriever _priceRetriever;
    private readonly TradingStrategy _tradingStrategy;

    public PriceRetrieverTests()
    {
        _priceRetriever = new PriceRetriever(_mockBinanceClient.Object, _mockLogger.Object);
        _tradingStrategy = new()
        {
            TargetProfit = 10m,
            Quantity = 200m,
            Interval = "1m",
            Period = 60,
            Symbol = "SOLUSDT",
            LimitBenefit = 1000,
            Discount = 0
        };
    }

    [TestMethod]
    public void GetRecentPricesValidKlinesReturnsClosingPrices()
    {
        // Arrange
        var klines = new List<List<object>>
        {
            new() { "100.5", "100.5", "100.5", "100.5", "100.6", "100.5" },
            new() { "100.5", "100.5", "100.5", "100.5", "101.6", "100.5" }
        };

        // Act
        var result = _priceRetriever.GetClosingPrices(klines);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(100.6m, result[0]);
        Assert.AreEqual(101.6m, result[1]);
    }

    [TestMethod]
    public void GetRecentPricesInvalidKlinesThrowsException()
    {
        // Arrange
        var klines = new List<List<object>>
        {
            new() { "100.5", "100.5", "100.5", "100.5", "not a number", "100.5" },
        };

        // Act & Assert
        Assert.ThrowsException<InvalidCastException>(() => _priceRetriever.GetClosingPrices(klines));
    }

    [TestMethod]
    public void GetRecentPricesHttpRequestExceptionReturnsEmptyList()
    {
        // Arrange
        var klines = new List<List<object>>();

        // Act
        var result = _priceRetriever.GetClosingPrices(klines);

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void ClosingPricesThrowsInvalidCastException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockBinanceClient = new Mock<IBinanceClient>();
        var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
        var klines = new List<List<object>>
        {
            new() { "100.5", "100.5", "100.5", "100.5", "invalid_data", "100.5" },
        };

        // Act & Assert
        Assert.ThrowsException<InvalidCastException>(() => priceRetriever.GetClosingPrices(klines));
    }

    [TestMethod]
    public async Task HandleDiscountAsyncTestDiscountUpdated()
    {
        // Arrange
        _mockBinanceClient.Setup(c => c.GetCommissionBySymbolAsync(_tradingStrategy.Symbol))
            .ReturnsAsync(new Commission
            {
                Discount = new Discount
                {
                    DiscountAsset = "BNB",
                    DiscountValue = "0.75"
                },
                StandardCommission = new CommissionRates
                {
                    Maker = "0.00100000",
                    Taker = "0.00100000",
                    Buyer = "0.00000000",
                    Seller = "0.00000000"
                }
            });
        _mockBinanceClient.Setup(c => c.GetAccountInfosAsync())
            .ReturnsAsync(new Account
            {
                Balances =
                [
                    new Balance
                    {
                        Asset = "SOL",
                        Free = "1"
                    },
                    new Balance
                    {
                        Asset = "USDT",
                        Free = "1"
                    },
                    new Balance
                    {
                        Asset = "BNB",
                        Free = "0.99927185"
                    },
                ]
            });
        _mockBinanceClient.Setup(c => c.GetPriceBySymbolAsync("BNBUSDT"))
            .ReturnsAsync(new Currency
            {
                Price = 300,
                Symbol = "BNBUSDT"
            });

        _mockLogger.Setup(c => c.WriteLog(It.IsAny<string>()));

        // Act
        await _priceRetriever.HandleDiscountAsync(_tradingStrategy);

        // Assert
        Assert.AreEqual(0.25m, _tradingStrategy.Discount);
    }

    [TestMethod]
    public async Task HandleDiscountAsyncTestDiscountEqualToZero()
    {
        // Arrange
        _mockBinanceClient.Setup(c => c.GetCommissionBySymbolAsync(_tradingStrategy.Symbol))
            .ReturnsAsync(new Commission
            {
                Discount = new Discount
                {
                    DiscountAsset = "BNB",
                    DiscountValue = "0.75"
                },
                StandardCommission = new CommissionRates
                {
                    Maker = "0.00100000",
                    Taker = "0.00100000",
                    Buyer = "0.00000000",
                    Seller = "0.00000000"
                }
            });
        _mockBinanceClient.Setup(c => c.GetAccountInfosAsync())
            .ReturnsAsync(new Account
            {
                Balances =
                [
                    new Balance
                    {
                        Asset = "SOL",
                        Free = "1"
                    },
                    new Balance
                    {
                        Asset = "USDT",
                        Free = "1"
                    },
                    new Balance
                    {
                        Asset = "BNB",
                        Free = "0.99927185"
                    }
                ]
            });
        _mockBinanceClient.Setup(c => c.GetPriceBySymbolAsync("BNBUSDT"))
            .ReturnsAsync(new Currency
            {
                Price = 10,
                Symbol = "BNBUSDT"
            });

        // Act
        await _priceRetriever.HandleDiscountAsync(_tradingStrategy);

        // Assert
        Assert.AreEqual(0, _tradingStrategy.Discount);
    }
}