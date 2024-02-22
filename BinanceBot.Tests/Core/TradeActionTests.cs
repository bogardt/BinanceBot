using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Model.Message;
using BinanceBot.Core;
using BinanceBot.Strategy;
using Moq;
using Newtonsoft.Json;

namespace BinanceBot.Tests.Core;

[TestClass]
public class TradeActionTests
{
    private readonly Mock<IBinanceClient> _mockBinanceClient = new();
    private readonly Mock<IPriceRetriever> _mockPriceRetriever = new();
    private readonly Mock<ITechnicalIndicatorsCalculator> _mockTechnicalIndicatorsCalculator = new();
    private readonly Mock<ILogger> _mockLogger = new();
    private readonly TradeAction _tradeAction;
    private readonly TradingStrategy _tradingStrategy = new() { TestMode = false };
    private readonly TradingStrategy _tradingStrategyTest = new();

    public TradeActionTests()
    {
        _tradeAction = new TradeAction(_mockBinanceClient.Object, _mockPriceRetriever.Object, _mockTechnicalIndicatorsCalculator.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task WaitBuyAsyncCompletesSuccessfullyTestStrategy()
    {
        // Arrange
        var orders = new List<Order>() { new() { Symbol = _tradingStrategyTest.Symbol, Side = "BUY" } };
        var ordersEnd = new List<Order>();
        _mockBinanceClient.SetupSequence(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol))
                          .ReturnsAsync(orders)
                          .ReturnsAsync(orders)
                          .ReturnsAsync(ordersEnd);

        // Act
        await _tradeAction.WaitBuyAsync(_tradingStrategyTest.Symbol);

        // Assert
        _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol), Times.Exactly(3));
    }

    [TestMethod]
    public async Task WaitSellAsyncCompletesSuccessfullyTestStrategy()
    {
        // Arrange
        var orders = new List<Order>() { new() { Symbol = _tradingStrategyTest.Symbol, Side = "SELL" } };
        var ordersEnd = new List<Order>();
        _mockBinanceClient.SetupSequence(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol))
                          .ReturnsAsync(orders)
                          .ReturnsAsync(orders)
                          .ReturnsAsync(ordersEnd);

        // Act
        await _tradeAction.WaitSellAsync(_tradingStrategyTest.Symbol);

        // Assert
        _mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol), Times.Exactly(3));
    }

    [TestMethod]
    public async Task BuyValidParametersCallsBinanceClient()
    {
        // Arrange
        decimal currentCurrencyPrice = 100m;
        decimal volatility = 0.05m;
        var order = new Order
        {
            OrderId = 1,
            Symbol = "BTCUSDT"
        };
        _mockBinanceClient.Setup(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "BUY")).ReturnsAsync(order);
        _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol)).ReturnsAsync(new List<Order>());

        // Act
        await _tradeAction.Buy(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);

        // Assert
        _mockBinanceClient.Verify(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "BUY"), Times.Once);
        _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(2));
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[ACHAT]"))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("real : " + JsonConvert.SerializeObject(order, Formatting.Indented)))), Times.AtLeastOnce);
        Assert.IsTrue(_tradingStrategy.OpenPosition);
    }

    [TestMethod]
    public async Task SellValidParametersCallsBinanceClient()
    {
        // Arrange
        var stopLossStrategy = new StopLossStrategy();
        decimal currentCurrencyPrice = 100m;
        decimal volatility = 0.05m;
        var order = new Order
        {
            OrderId = 1,
            Symbol = "BTCUSDT"
        };
        _tradingStrategy.TotalBenefit = 100;
        _tradingStrategy.LimitBenefit = 100;
        _mockTechnicalIndicatorsCalculator.Setup(c => c.DetermineLossStrategy(_tradingStrategy.CryptoPurchasePrice, volatility)).Returns(10000);
        _mockBinanceClient.Setup(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "SELL")).ReturnsAsync(order);
        _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol)).ReturnsAsync(new List<Order>());

        // Act
        var (prixVenteCible, maxBenefitDone) = await _tradeAction.Sell(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);

        // Assert
        _mockBinanceClient.Verify(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "SELL"), Times.Once);
        _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(4));
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("STOPP LOSS"))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("real : " + JsonConvert.SerializeObject(order, Formatting.Indented)))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[VENTE]"))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("BENEFICE LIMITE"))), Times.AtLeastOnce);
        Assert.IsTrue(_tradingStrategy.TotalBenefit >= _tradingStrategy.LimitBenefit);
        Assert.IsTrue(!_tradingStrategy.OpenPosition);
    }


    [TestMethod]
    public async Task SellValidParametersCallsBinanceClientBenefitHasNotBeenReached()
    {
        // Arrange
        var stopLossStrategy = new StopLossStrategy();
        decimal currentCurrencyPrice = 10m;
        decimal volatility = 0.05m;
        var order = new Order
        {
            OrderId = 1,
            Symbol = "BTCUSDT"
        };
        _mockTechnicalIndicatorsCalculator.Setup(c => c.DetermineLossStrategy(_tradingStrategy.CryptoPurchasePrice, volatility)).Returns(10000);
        _mockBinanceClient.Setup(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "SELL")).ReturnsAsync(order);
        _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol)).ReturnsAsync(new List<Order>());

        // Act
        var (prixVenteCible, maxBenefitDone) = await _tradeAction.Sell(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);

        // Assert
        _mockBinanceClient.Verify(c => c.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, currentCurrencyPrice, "SELL"), Times.Once);
        _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(3));
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("STOPP LOSS"))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("real : " + JsonConvert.SerializeObject(order, Formatting.Indented)))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[VENTE]"))), Times.AtLeastOnce);
        Assert.IsTrue(_tradingStrategy.TotalBenefit < _tradingStrategy.LimitBenefit);
        Assert.IsTrue(!_tradingStrategy.OpenPosition);
    }

    [TestMethod]
    public async Task SellSkipSelling()
    {
        // Arrange
        var stopLossStrategy = new StopLossStrategy();
        decimal currentCurrencyPrice = 100m;
        decimal volatility = 0.05m;
        decimal minimumSellingPrice = currentCurrencyPrice + 100;
        var order = new Order
        {
            OrderId = 1,
            Symbol = "BTCUSDT"
        };
        _tradingStrategy.CryptoPurchasePrice = 100m;
        _tradingStrategy.OpenPosition = true;
        _mockPriceRetriever.Setup(c => c.CalculateMinimumSellingPrice(_tradingStrategy.CryptoPurchasePrice, _tradingStrategy.Quantity, _tradingStrategy.FeePercentage, _tradingStrategy.Discount, _tradingStrategy.TargetProfit)).Returns(minimumSellingPrice);
        _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategy.Symbol)).ReturnsAsync(new List<Order>());

        // Act
        var (prixVenteCible, maxBenefitDone) = await _tradeAction.Sell(_tradingStrategy, currentCurrencyPrice, volatility, _tradingStrategy.Symbol);

        // Assert
        _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Never);
        Assert.IsTrue(maxBenefitDone == false);
        Assert.IsTrue(_tradingStrategy.OpenPosition);
        Assert.IsFalse(currentCurrencyPrice >= minimumSellingPrice);
    }

    [TestMethod]
    public async Task BuyValidParametersCallsBinanceClientTestStrategy()
    {
        // Arrange
        decimal currentCurrencyPrice = 100m;
        decimal volatility = 0.05m;
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
        _mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(_tradingStrategyTest.Symbol, _tradingStrategyTest.Quantity, currentCurrencyPrice, "BUY")).ReturnsAsync(testOrder);
        _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol)).ReturnsAsync(new List<Order>());

        // Act
        await _tradeAction.Buy(_tradingStrategyTest, currentCurrencyPrice, volatility, _tradingStrategyTest.Symbol);

        // Assert
        _mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(_tradingStrategyTest.Symbol, _tradingStrategyTest.Quantity, currentCurrencyPrice, "BUY"), Times.Once);
        _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(2));
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[ACHAT]"))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("test : " + JsonConvert.SerializeObject(testOrder, Formatting.Indented)))), Times.AtLeastOnce);
        Assert.IsTrue(_tradingStrategyTest.OpenPosition);
    }

    [TestMethod]
    public async Task SellValidParametersCallsBinanceClientTestStrategy()
    {
        // Arrange
        var stopLossStrategy = new StopLossStrategy();
        decimal currentCurrencyPrice = 100m;
        decimal volatility = 0.05m;
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
        _mockTechnicalIndicatorsCalculator.Setup(c => c.DetermineLossStrategy(_tradingStrategyTest.CryptoPurchasePrice, volatility)).Returns(10000);
        _mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(_tradingStrategyTest.Symbol, _tradingStrategyTest.Quantity, currentCurrencyPrice, "SELL")).ReturnsAsync(testOrder);
        _mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(_tradingStrategyTest.Symbol)).ReturnsAsync(new List<Order>());

        // Act
        var (prixVenteCible, maxBenefitDone) = await _tradeAction.Sell(_tradingStrategyTest, currentCurrencyPrice, volatility, _tradingStrategyTest.Symbol);

        // Assert
        _mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(_tradingStrategyTest.Symbol, _tradingStrategyTest.Quantity, currentCurrencyPrice, "SELL"), Times.Once);
        _mockLogger.Verify(c => c.WriteLog(It.IsAny<string>()), Times.Exactly(4));
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("STOPP LOSS"))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("test : " + JsonConvert.SerializeObject(testOrder, Formatting.Indented)))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("[VENTE]"))), Times.AtLeastOnce);
        _mockLogger.Verify(c => c.WriteLog(It.Is<string>(s => s.Contains("BENEFICE LIMITE"))), Times.AtLeastOnce);
        Assert.IsTrue(_tradingStrategyTest.TotalBenefit >= _tradingStrategyTest.LimitBenefit);
        Assert.IsTrue(!_tradingStrategyTest.OpenPosition);
    }
}