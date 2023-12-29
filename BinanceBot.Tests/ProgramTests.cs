using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
using BinanceBot.Strategy;
using BinanceBot.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace BinanceBot.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void ServiceConfigurationValidConfigurationServicesCanBeResolved()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IBinanceClient, BinanceClient>();
                    services.AddSingleton<ITradeAction, TradeAction>();
                    services.AddSingleton<IFileSystem, FileSystem>();
                    services.AddSingleton<IVolatilityStrategy, VolatilityStrategy>();
                    services.AddSingleton<ITechnicalIndicatorsCalculator, TechnicalIndicatorsCalculator>();
                    services.AddSingleton<IPriceRetriever, PriceRetriever>();
                    services.AddSingleton<IMarketTradeHandler, MarketTradeHandler>();
                    services.AddSingleton<ILogger, Logger>();
                    services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
                })
                .Build();

            using var serviceScope = host.Services.CreateScope();
            var provider = serviceScope.ServiceProvider;

            Assert.IsNotNull(provider.GetService<IBinanceClient>());
            Assert.IsNotNull(provider.GetService<ITradeAction>());
            Assert.IsNotNull(provider.GetService<IFileSystem>());
            Assert.IsNotNull(provider.GetService<IVolatilityStrategy>());
            Assert.IsNotNull(provider.GetService<ITechnicalIndicatorsCalculator>());
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
            var mockBinanceClient = new Mock<IBinanceClient>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockVolatilityStrategy = new Mock<IVolatilityStrategy>();
            var mockTechnicalIndicatorsCalculator = new Mock<ITechnicalIndicatorsCalculator>();
            var mockLogger = new Mock<ILogger>();
            var mockHttpClientWrapper = new Mock<IHttpClientWrapper>();

            //
            var priceRetriever = new PriceRetriever(mockBinanceClient.Object, mockLogger.Object);
            var tradeAction = new TradeAction(mockBinanceClient.Object, mockVolatilityStrategy.Object, priceRetriever, mockLogger.Object);
            var marketTradeHandler = new MarketTradeHandler(mockBinanceClient.Object, mockVolatilityStrategy.Object, mockTechnicalIndicatorsCalculator.Object, priceRetriever, tradeAction, mockLogger.Object, tradingStrategy);

            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IBinanceClient>(mockBinanceClient.Object);
                    services.AddSingleton<ITradeAction>(tradeAction);
                    services.AddSingleton<IFileSystem>(mockFileSystem.Object);
                    services.AddSingleton<IVolatilityStrategy>(mockVolatilityStrategy.Object);
                    services.AddSingleton<ITechnicalIndicatorsCalculator>(mockTechnicalIndicatorsCalculator.Object);
                    services.AddSingleton<IPriceRetriever>(priceRetriever);
                    services.AddSingleton<IMarketTradeHandler>(marketTradeHandler);
                    services.AddSingleton<ILogger>(mockLogger.Object);
                    services.AddSingleton<IHttpClientWrapper>(mockHttpClientWrapper.Object);
                })
                .Build();

            using var serviceScope = host.Services.CreateScope();
            var provider = serviceScope.ServiceProvider;
            var binanceBot = provider.GetRequiredService<IMarketTradeHandler>();

            var interval = tradingStrategy.Interval;
            var period = tradingStrategy.Period;
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var klines = Enumerable.Repeat(kline, period).ToList();

            mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(tradingStrategy.Symbol, interval, period.ToString()))
                              .ReturnsAsync(klines);
            mockBinanceClient.Setup(c => c.GetKLinesBySymbolAsync(tradingStrategy.Symbol, interval, (period + 1).ToString()))
                              .ReturnsAsync(klines);

            mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateRSI(klines, period))
                .Returns(30m);

            mockTechnicalIndicatorsCalculator.Setup(c => c.CalculateMovingAverage(klines, period))
                .Returns(100m);

            mockVolatilityStrategy.Setup(c => c.CalculateVolatility(It.IsAny<List<List<object>>>()))
                .Returns(0.25m);

            var currencyForBuy = new Currency { Symbol = tradingStrategy.Symbol, Price = 90m };
            var currencyForSell = new Currency { Symbol = tradingStrategy.Symbol, Price = 100m };
            mockBinanceClient.SetupSequence(c => c.GetPriceBySymbolAsync(tradingStrategy.Symbol))
                              .ReturnsAsync(currencyForBuy)
                              .ReturnsAsync(currencyForSell);

            mockBinanceClient.Setup(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                              .ReturnsAsync(It.IsAny<TestOrder>());

            var orders = new List<Order>();
            mockBinanceClient.Setup(c => c.GetOpenOrdersAsync(tradingStrategy.Symbol))
                              .ReturnsAsync(orders);

            mockBinanceClient.Setup(c => c.GetAccountInfosAsync())
                .ReturnsAsync(new Account
                {
                    Balances = new []
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
                });

            mockBinanceClient.Setup(c => c.GetPriceBySymbolAsync("BNBUSDT"))
                .ReturnsAsync(new Currency
                {
                    Price = 100m
                });

            mockBinanceClient.Setup(c => c.GetCommissionBySymbolAsync(tradingStrategy.Symbol))
                .ReturnsAsync(new Commission
                {
                    StandardCommission = new CommissionRates
                    {
                        Maker = "0.001"
                    },
                    TaxCommission = new CommissionRates
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

            mockTechnicalIndicatorsCalculator.Verify(c => c.CalculateRSI(klines, period), Times.Exactly(2));
            mockVolatilityStrategy.Verify(c => c.CalculateVolatility(It.IsAny<List<List<object>>>()), Times.Exactly(2));
            mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(tradingStrategy.Symbol), Times.Exactly(2));
            mockBinanceClient.Verify(c => c.GetKLinesBySymbolAsync(tradingStrategy.Symbol, interval, period.ToString()), Times.Exactly(2));
            mockBinanceClient.Verify(c => c.GetPriceBySymbolAsync(tradingStrategy.Symbol), Times.Exactly(2));
            mockBinanceClient.Verify(c => c.PlaceTestOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
            mockBinanceClient.Verify(c => c.GetOpenOrdersAsync(tradingStrategy.Symbol), Times.Exactly(2));
        }
    }
}
