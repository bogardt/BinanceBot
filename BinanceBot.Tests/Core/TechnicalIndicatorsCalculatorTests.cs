using BinanceBot.Abstraction;
using BinanceBot.Core;
using Moq;

namespace BinanceBot.Tests.Core;

[TestClass]
public class TechnicalIndicatorsCalculatorTests
{
    private readonly Mock<IPriceRetriever> _mockPriceRetriever = new();
    private readonly TechnicalIndicatorsCalculator _technicalIndicatorsCalculator;

    public TechnicalIndicatorsCalculatorTests()
    {
        _technicalIndicatorsCalculator = new();
    }

    [TestMethod]
    public void CalculateMovingAverageValidKlinesCalculatesAverage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockBinanceClient = new Mock<ICryptoMarketHttpClient>();
        var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
        var period = 2;
        var klines = new List<List<object>>
        {
            new() { "100.5", "100.5", "100.5", "100.5", "100", "100.5" },
            new() { "100.5", "100.5", "100.5", "100.5", "200", "100.5" }
        };
        var closingPrices = priceRetriever.GetClosingPrices(klines);

        // Act
        var result = _technicalIndicatorsCalculator.CalculateMovingAverage(closingPrices, period);

        // Assert
        Assert.AreEqual(150m, result);
    }

    [TestMethod]
    public void CalculateRSIValidKlinesCalculatesRSI()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockBinanceClient = new Mock<ICryptoMarketHttpClient>();
        var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
        var technicalIndicatorsCalculator = new TechnicalIndicatorsCalculator();
        var period = 60;
        var klines = CreateLines(period);
        var closingPrices = priceRetriever.GetClosingPrices(klines);

        // Act
        var result = technicalIndicatorsCalculator.CalculateRSI(closingPrices, period);

        // Assert
        Assert.IsTrue(result < 50);
        Assert.IsTrue(result > 49);


        static List<List<object>> CreateLines(int period)
        {
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var kline2 = new List<object> { 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m };
            var klines = new List<List<object>>();


            for (var i = 0; i < period; i++)
            {
                klines.Add(i % 2 == 0 ? kline : kline2);
            }

            return klines;
        }
    }



    [TestMethod]
    public void CalculateRSIWhenPerteMoyenneIsZeroReturnsZero()
    {
        // Arrange
        _mockPriceRetriever.Setup(m => m.GetClosingPrices(It.IsAny<List<List<object>>>()))
            .Returns(new List<decimal> { 100, 100, 100, 100, 100 });
        int period = 5;

        // Act
        var result = _technicalIndicatorsCalculator.CalculateRSI(new List<decimal>(), period);

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public void CalculateRSIWhenPerteMoyenneIsNotZeroReturnsCalculatedValue()
    {
        // Arrange
        int period = 5;

        // Act
        var result = _technicalIndicatorsCalculator.CalculateRSI(new List<decimal> { 100, 102, 98, 101, 103 }, period);

        // Assert
        decimal expectedRsi = 63.64m;
        Assert.AreEqual(expectedRsi, result, 0.01m);
    }

    [TestMethod]
    public void CalculateVolatilityValidKlinesCalculatesVolatility()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var mockBinanceClient = new Mock<ICryptoMarketHttpClient>();
        var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
        var klines = new List<List<object>>
        {
            new() { "100.5", "100.5", "100.5", "100.5", "100.6", "100.5" },
            new() { "100.5", "100.5", "100.5", "100.5", "101.6", "100.5" }
        };
        var closingPrices = priceRetriever.GetClosingPrices(klines);

        // Act
        var result = _technicalIndicatorsCalculator.CalculateVolatility(closingPrices);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void DetermineLossStrategyGivenVolatilityAndConfigCalculatesStopLossPrice()
    {
        // Arrange
        decimal volatility = 0.05m;
        decimal cryptoPurchasePrice = 100m;

        // Act
        var result = _technicalIndicatorsCalculator.DetermineLossStrategy(cryptoPurchasePrice, volatility);

        // Assert
        Assert.IsTrue(result < cryptoPurchasePrice);
    }
}