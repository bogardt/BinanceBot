using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model;
using BinanceBot.Core;
using Moq;
using TradingCalculation;
using TradingCalculation.Strategy;

namespace BinanceBot.Tests.Core;

[TestClass]
public class MarketTradeHandlerTests
{
    private readonly MarketTradeHandler _handler;
    private readonly TradeAction _tradeAction;
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly Mock<IExchangeHttpClient> _mockBinanceClient = new();
    private readonly Mock<ITechnicalIndicatorsCalculator> _mockTechnicalIndicatorsCalculator = new();
    private readonly Mock<IPriceRetriever> _mockPriceRetriever = new();
    private static readonly TradingStrategy _tradingStrategy = new()
    {
        TargetProfit = 10m,
        Quantity = 200m,
        Interval = "1m",
        Period = 60,
        Symbol = "SOLUSDT",
        LimitBenefit = 1000,
        Discount = 0.25m
    };

    public MarketTradeHandlerTests()
    {
        _tradeAction = new TradeAction(_mockBinanceClient.Object,
            _mockTechnicalIndicatorsCalculator.Object,
            _mockLogger.Object);
        _handler = new MarketTradeHandler(_mockBinanceClient.Object,
            _mockTechnicalIndicatorsCalculator.Object,
            _mockPriceRetriever.Object,
            _tradeAction,
            _mockLogger.Object,
            _tradingStrategy);
    }

    [TestMethod]
    public async Task TradeOnLimitAsyncValidTradeScenario()
    {
        // Arrange
        var interval = _tradingStrategy.Interval;
        var period = _tradingStrategy.Period;
        var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
        var klines = Enumerable.Repeat(kline, period).ToList();

        _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, period.ToString()))
                          .ReturnsAsync(klines);
        _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, (period + 1).ToString()))
                          .ReturnsAsync(klines);

        _mockPriceRetriever.Setup(c => c.HandleDiscountAsync(_tradingStrategy))
                           .Returns(Task.CompletedTask);

        _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateMinimumSellingPrice(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
                           .Returns(95);
        _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateRSI(It.IsAny<List<decimal>>(), period))
                           .Returns(30m);

        _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateMovingAverage(It.IsAny<List<decimal>>(), period))
                           .Returns(100m);

        _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateVolatility(It.IsAny<List<decimal>>()))
                                    .Returns(0.25m);

        _mockTechnicalIndicatorsCalculator.Setup(c => c.IsTargetPriceAchievable(It.IsAny<decimal>(), It.IsAny<List<decimal>>()))
                                    .Returns(true);

        var currencyForBuy = new Currency { Symbol = _tradingStrategy.Symbol, Price = 90m };
        var currencyForSell = new Currency { Symbol = _tradingStrategy.Symbol, Price = 100m };
        _mockBinanceClient.SetupSequence(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol))
                          .ReturnsAsync(currencyForBuy)
                          .ReturnsAsync(currencyForSell);

        _mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                          .ReturnsAsync(new TestOrder());

        var orders = new List<Order>();
        _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol))
                          .ReturnsAsync(orders);

        // Act
        await _handler.TradeOnLimitAsync();

        // Assert
        _mockTechnicalIndicatorsCalculator.Verify(c => c.CalculateRSI(It.IsAny<List<decimal>>(), period), Times.Exactly(2));
        _mockTechnicalIndicatorsCalculator.Verify(c => c.CalculateVolatility(It.IsAny<List<decimal>>()), Times.Exactly(2));
        _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol), Times.Exactly(2));
        _mockBinanceClient.Verify(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, period.ToString()), Times.Exactly(2));
        _mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol), Times.Exactly(2));
        _mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
        _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(6));
        Assert.IsTrue(_tradingStrategy.TotalBenefit >= _tradingStrategy.LimitBenefit);
        //_mockLogger.Verify(c => c.WriteLog(It.Is<string>(str => str.Contains("diffMarge") && str.Contains("forecastTargetPrice"))), Times.AtLeastOnce());
    }


    [TestMethod]
    public async Task TradeOnLimitAsyncValidTradeScenario2()
    {
        // Arrange
        var interval = _tradingStrategy.Interval;
        var period = _tradingStrategy.Period;
        var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
        var klines = Enumerable.Repeat(kline, period).ToList();

        _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, period.ToString()))
                          .ReturnsAsync(klines);
        _mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, (period + 1).ToString()))
                          .ReturnsAsync(klines);

        _mockPriceRetriever.Setup(c => c.HandleDiscountAsync(_tradingStrategy))
                           .Returns(Task.CompletedTask);
        _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateMinimumSellingPrice(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<decimal>()))
               .Returns(99);

        _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateRSI(It.IsAny<List<decimal>>(), period))
            .Returns(30m);

        _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateMovingAverage(It.IsAny<List<decimal>>(), period))
            .Returns(100m);

        _mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateVolatility(It.IsAny<List<decimal>>()))
            .Returns(0.25m);

        _mockTechnicalIndicatorsCalculator.Setup(c => c.IsTargetPriceAchievable(It.IsAny<decimal>(), It.IsAny<List<decimal>>()))
            .Returns(true);

        var currencyForBuy1 = new Currency { Symbol = _tradingStrategy.Symbol, Price = 110m };
        var currencyForBuy2 = new Currency { Symbol = _tradingStrategy.Symbol, Price = 90m };
        var currencyForSell2 = new Currency { Symbol = _tradingStrategy.Symbol, Price = 100m };
        _mockBinanceClient.SetupSequence(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol))
                          .ReturnsAsync(currencyForBuy1)
                          .ReturnsAsync(currencyForBuy2)
                          .ReturnsAsync(currencyForSell2);

        _mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                          .ReturnsAsync(new TestOrder());

        var orders = new List<Order>();
        _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol))
                          .ReturnsAsync(orders);

        // Act
        await _handler.TradeOnLimitAsync();

        // Assert
        _mockTechnicalIndicatorsCalculator.Verify(c => c.CalculateRSI(It.IsAny<List<decimal>>(), period), Times.Exactly(3));
        _mockTechnicalIndicatorsCalculator.Verify(c => c.CalculateVolatility(It.IsAny<List<decimal>>()), Times.Exactly(3));
        _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol), Times.Exactly(2));
        _mockBinanceClient.Verify(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, period.ToString()), Times.Exactly(3));
        _mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol), Times.Exactly(3));
        _mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
        _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(7));
        Assert.IsTrue(_tradingStrategy.TotalBenefit >= _tradingStrategy.LimitBenefit);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(str => str.Contains("diffMarge") && str.Contains("forecastTargetPrice"))), Times.AtLeastOnce());
    }

    [TestMethod]
    public async Task TradeOnLimitAsyncWhenApiThrowsExceptionHandlesException()
    {
        // Arrange
        var interval = _tradingStrategy.Interval;
        var period = _tradingStrategy.Period;

        var mockBinanceClient = new Mock<IExchangeHttpClient>();
        mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(_tradingStrategy.Symbol, interval, period.ToString()))
                          .ReturnsAsync(new List<List<object>>());
        mockBinanceClient.Setup(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol))
                          .ThrowsAsync(new Exception("API Error"));

        _mockPriceRetriever.Setup(c => c.HandleDiscountAsync(_tradingStrategy))
            .Returns(Task.CompletedTask);

        var marketTradeHandler = new MarketTradeHandler(mockBinanceClient.Object,
                                                        _mockTechnicalIndicatorsCalculator.Object,
                                                        _mockPriceRetriever.Object,
                                                        _tradeAction,
                                                        _mockLogger.Object,
                                                        _tradingStrategy);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => marketTradeHandler.TradeOnLimitAsync());
        mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(_tradingStrategy.Symbol), Times.Once);
    }
}
