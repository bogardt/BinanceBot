using BinanceBot.Abstraction;
using BinanceBot.Core;
using BinanceBot.Model;
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
        public void ServiceConfiguration_ValidConfiguration_ServicesCanBeResolved()
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
        public async Task MainFlow_RunsSuccessfully()
        {
            var binanceClientMock = new Mock<IBinanceClient>();
            var fileSystemMock = new Mock<IFileSystem>();
            var volatilityStrategyMock = new Mock<IVolatilityStrategy>();
            var technicalIndicatorsCalculatorMock = new Mock<ITechnicalIndicatorsCalculator>();
            var priceRetrieverMock = new Mock<IPriceRetriever>();
            var loggerMock = new Mock<ILogger>();
            var httpClientWrapperMock = new Mock<IHttpClientWrapper>();

            //
            var tradeActionMock = new TradeAction(binanceClientMock.Object, volatilityStrategyMock.Object, loggerMock.Object);
            var marketTradeHandler = new MarketTradeHandler(binanceClientMock.Object, volatilityStrategyMock.Object, technicalIndicatorsCalculatorMock.Object, tradeActionMock, loggerMock.Object, new TradingConfig(TradeSetup.Dict, TradeSetup.Symbol));


            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IBinanceClient>(binanceClientMock.Object);
                    services.AddSingleton<ITradeAction>(tradeActionMock);
                    services.AddSingleton<IFileSystem>(fileSystemMock.Object);
                    services.AddSingleton<IVolatilityStrategy>(volatilityStrategyMock.Object);
                    services.AddSingleton<ITechnicalIndicatorsCalculator>(technicalIndicatorsCalculatorMock.Object);
                    services.AddSingleton<IPriceRetriever>(priceRetrieverMock.Object);
                    services.AddSingleton<IMarketTradeHandler>(marketTradeHandler);
                    services.AddSingleton<ILogger>(loggerMock.Object);
                    services.AddSingleton<IHttpClientWrapper>(httpClientWrapperMock.Object);
                })
                .Build();

            using var serviceScope = host.Services.CreateScope();
            var provider = serviceScope.ServiceProvider;
            var binanceBot = provider.GetRequiredService<IMarketTradeHandler>();

            var interval = TradeSetup.Dict[TradeSetup.Symbol].Interval;
            var period = TradeSetup.Dict[TradeSetup.Symbol].Period;
            var kline = new List<object> { 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m, 100m };
            var klines = Enumerable.Repeat(kline, period).ToList();

            binanceClientMock.Setup(c => c.GetKLinesBySymbolAsync(TradeSetup.Symbol, interval, period.ToString()))
                              .ReturnsAsync(klines);
            binanceClientMock.Setup(c => c.GetKLinesBySymbolAsync(TradeSetup.Symbol, interval, (period + 1).ToString()))
                              .ReturnsAsync(klines);

            technicalIndicatorsCalculatorMock.Setup(c => c.CalculateRSI(klines, period))
                .Returns(30m);

            technicalIndicatorsCalculatorMock.Setup(c => c.CalculateMovingAverage(klines, period))
                .Returns(100m);

            volatilityStrategyMock.Setup(c => c.CalculateVolatility(It.IsAny<List<List<object>>>()))
                .Returns(0.25m);

            var currencyForBuy = new Currency { Symbol = TradeSetup.Symbol, Price = 90m };
            var currencyForSell = new Currency { Symbol = TradeSetup.Symbol, Price = 100m };
            binanceClientMock.SetupSequence(c => c.GetPriceBySymbolAsync(TradeSetup.Symbol))
                              .ReturnsAsync(currencyForBuy)
                              .ReturnsAsync(currencyForSell);

            binanceClientMock.Setup(c => c.PlaceOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()))
                              .ReturnsAsync(string.Empty);

            var orders = new List<Order>();
            binanceClientMock.Setup(c => c.GetOpenOrdersAsync(TradeSetup.Symbol))
                              .ReturnsAsync(orders);

            // run bot
            await binanceBot.TradeOnLimitAsync();


            technicalIndicatorsCalculatorMock.Verify(c => c.CalculateRSI(klines, period), Times.Exactly(2));
            volatilityStrategyMock.Verify(c => c.CalculateVolatility(It.IsAny<List<List<object>>>()), Times.Exactly(2));
            binanceClientMock.Verify(c => c.GetOpenOrdersAsync(TradeSetup.Symbol), Times.Exactly(2));
            binanceClientMock.Verify(c => c.GetKLinesBySymbolAsync(TradeSetup.Symbol, interval, period.ToString()), Times.Exactly(2));
            binanceClientMock.Verify(c => c.GetPriceBySymbolAsync(TradeSetup.Symbol), Times.Exactly(2));
            binanceClientMock.Verify(c => c.PlaceOrderAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Exactly(2));
            binanceClientMock.Verify(c => c.GetOpenOrdersAsync(TradeSetup.Symbol), Times.Exactly(2));
        }
    }
}
