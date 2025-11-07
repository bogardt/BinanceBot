using BinanceBot.Abstraction;
using BinanceBot.BinanceApi;
using BinanceBot.BinanceApi.Model;
using BinanceBot.BinanceApi.Validation;
using BinanceBot.Core;
using BinanceBot.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using TradingCalculation;
using TradingCalculation.Strategy;

namespace BinanceBot.Tests;

[TestClass]
public class ProgramTests
{
    [TestMethod]
    public void ServiceConfigurationValidConfigurationServicesCanBeResolved()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IExchangeHttpClient, BinanceClient>();
                services.AddSingleton<ITradeAction, TradeAction>();
                services.AddSingleton<IFileSystem, FileSystem>();
                services.AddSingleton<ITechnicalIndicatorsCalculator, TechnicalIndicatorsCalculator>();
                services.AddSingleton<IApiValidatorService, ApiValidatorService>();
                services.AddSingleton<IPriceRetriever, PriceRetriever>();
                services.AddSingleton<IMarketTradeHandler, MarketTradeHandler>();
                services.AddSingleton<ILogger, Logger>();
                services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
            })
            .Build();

        using var serviceScope = host.Services.CreateScope();
        var provider = serviceScope.ServiceProvider;

        Assert.IsNotNull(provider.GetService<IExchangeHttpClient>());
        Assert.IsNotNull(provider.GetService<ITradeAction>());
        Assert.IsNotNull(provider.GetService<IFileSystem>());
        Assert.IsNotNull(provider.GetService<ITechnicalIndicatorsCalculator>());
        Assert.IsNotNull(provider.GetService<IApiValidatorService>());
        Assert.IsNotNull(provider.GetService<IPriceRetriever>());
        Assert.IsNotNull(provider.GetService<IMarketTradeHandler>());
        Assert.IsNotNull(provider.GetService<ILogger>());
        Assert.IsNotNull(provider.GetService<IHttpClientWrapper>());
    }

    [TestMethod]
    public async Task MainFlowRunsSuccessfully()
    {
        var tradingStrategy = new TradingStrategy
        {
            TargetProfit = 10m,
            Quantity = 200m,
            Interval = "1m",
            Period = 60,
            Symbol = "SOLUSDT",
            LimitBenefit = 1000,
        };
        var mockBinanceClient = new Mock<IExchangeHttpClient>();
        var mockFileSystem = new Mock<IFileSystem>();
        var mockTechnicalIndicatorsCalculator = new Mock<ITechnicalIndicatorsCalculator>();
        var mockLogger = new Mock<ILogger>();
        var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();
        var mockApiValidatorService = new Mock<IApiValidatorService>();

        //
        var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
        var tradeAction = new TradeAction(mockBinanceClient.Object, new TechnicalIndicatorsCalculator(), mockLogger.Object);
        var marketTradeHandler = new MarketTradeHandler(mockBinanceClient.Object, mockTechnicalIndicatorsCalculator.Object, priceRetriever, tradeAction, mockLogger.Object, tradingStrategy);

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(mockBinanceClient.Object);
                services.AddSingleton<ITradeAction>(tradeAction);
                services.AddSingleton(mockFileSystem.Object);
                services.AddSingleton(mockApiValidatorService.Object);
                services.AddSingleton(mockTechnicalIndicatorsCalculator.Object);
                services.AddSingleton<IPriceRetriever>(priceRetriever);
                services.AddSingleton<IMarketTradeHandler>(marketTradeHandler);
                services.AddSingleton(mockLogger.Object);
                services.AddSingleton(mockHttpClientWrapper.Object);
            })
            .Build();

        using var serviceScope = host.Services.CreateScope();
        var provider = serviceScope.ServiceProvider;
        var binanceBot = provider.GetRequiredService<IMarketTradeHandler>();

        var interval = tradingStrategy.Interval;
        var period = tradingStrategy.Period;
        var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
        var klines = Enumerable.Repeat(kline, period).ToList();
        var closingPrices = priceRetriever.GetClosingPrices(klines);

        mockApiValidatorService.Setup(c => c.ValidateAsync<Account>(It.IsAny<string>()))
            .ReturnsAsync(It.IsAny<Account>());

        mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(tradingStrategy.Symbol, interval, period.ToString()))
                          .ReturnsAsync(klines);
        mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(tradingStrategy.Symbol, interval, (period + 1).ToString()))
                          .ReturnsAsync(klines);

        mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateRSI(closingPrices, period))
            .Returns(30m);

        mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateMovingAverage(closingPrices, period))
            .Returns(100m);

        mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateVolatility(It.IsAny<List<decimal>>()))
            .Returns(0.25m);

        mockTechnicalIndicatorsCalculator.Setup(c => c.IsTargetPriceAchievable(It.IsAny<decimal>(), It.IsAny<List<decimal>>()))
            .Returns(true);

        var currencyForBuy = new Currency { Symbol = tradingStrategy.Symbol, Price = 90m };
        var currencyForSell = new Currency { Symbol = tradingStrategy.Symbol, Price = 100m };
        mockBinanceClient.SetupSequence(c => c.GetPriceBySymbolAsync(tradingStrategy.Symbol))
                          .ReturnsAsync(currencyForBuy)
                          .ReturnsAsync(currencyForSell);

        mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                          .ReturnsAsync(new TestOrder());

        var orders = new List<Order>();
        mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(tradingStrategy.Symbol))
                          .ReturnsAsync(orders);

        mockBinanceClient.Setup(c => c.GetAccountInfosAsync())
            .ReturnsAsync(new Account
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
            });

        mockBinanceClient.Setup(c => c.GetPriceBySymbolAsync("BNBUSDT"))
            .ReturnsAsync(new Currency
            {
                Price = 100m
            });

        mockBinanceClient.Setup(c => c.GetCommissionBySymbolAsync(tradingStrategy.Symbol))
            .ReturnsAsync(new Commission
            {
                StandardCommission = new CommissionRate
                {
                    Maker = "0.001"
                },
                TaxCommission = new CommissionRate
                {
                    Maker = "0.000"
                },
                Discount = new Discount
                {
                    DiscountValue = "0.25",
                    EnabledForAccount = true,
                    EnabledForSymbol = true
                }
            });

        // run bot
        await binanceBot.TradeOnLimitAsync();

        mockTechnicalIndicatorsCalculator.Verify(c => c.CalculateRSI(closingPrices, period), Times.Exactly(2));
        mockTechnicalIndicatorsCalculator.Verify(c => c.CalculateVolatility(It.IsAny<List<decimal>>()), Times.Exactly(2));
        mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(tradingStrategy.Symbol), Times.Exactly(2));
        mockBinanceClient.Verify(c => c.GetKLinesBySymbolAsync(tradingStrategy.Symbol, interval, period.ToString()), Times.Exactly(2));
        mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(tradingStrategy.Symbol), Times.Exactly(2));
        mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
        mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(tradingStrategy.Symbol), Times.Exactly(2));
    }
}
