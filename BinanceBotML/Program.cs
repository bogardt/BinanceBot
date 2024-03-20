using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BinanceBotML;
using BinanceBot.Utils;
using BinanceBot.Abstraction;
using BinanceBot.BinanceApi;
using Microsoft.Extensions.Configuration;
using BinanceBot.BinanceApi.Validation;
using BinanceBot.Core;
using BinanceBot.BinanceApi.Validation.Validator;
using FluentValidation;
using TradingCalculation;

internal class Program
{
    private static async Task Main(string[] args)
    {
        static async Task RunBot(IServiceProvider services, string scope)
        {
            Console.WriteLine($"{scope}...");

            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var helper = new Helper(new FileSystem());
            var solutionPath = helper.GetSolutionPath();

            //var analyzer = provider.GetRequiredService<IAnalyzerML>();
            //var analyzer = new AnalyzerML();
            //var dt = analyzer.Train(solutionPath + "\\test.csv");
            //analyzer.Predict(dt.Item1, dt.Item2);
            var binance = provider.GetRequiredService<IExchangeHttpClient>();
            var klines = await binance.GetKLinesBySymbolAsync("SOLUSDT", "1s", 600.ToString());
            var technicalIndicatorsCalculator = provider.GetRequiredService<ITechnicalIndicatorsCalculator>();
            var tradingBot = new TradingBot(solutionPath + "\\test.csv");
            var closingPrices = klines.Select(x => x);
            var sma = technicalIndicatorsCalculator.CalculateMovingAverage(, klines.Count);

            for (var i = 0; i < 200; i++)
            {
                var price = await binance.GetPriceBySymbolAsync("SOLUSDT");
                Console.WriteLine(price.Price.ToString());
                tradingBot.MakeTradingDecision((float)price.Price);
                Task.Delay(1000).Wait();

            }
        }
        var helper = new Helper(new FileSystem());
        var solutionPath = helper.GetSolutionPath();

        var config = new ConfigurationBuilder()
            .SetBasePath(solutionPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {

                services.AddValidatorsFromAssemblyContaining<AccountValidator>();
                //services.AddScoped(typeof(IApiValidatorService), typeof(ApiValidatorService));

                //services.AddSingleton<IAnalyzerML, AnalyzerML>();
                services.AddSingleton<IApiValidatorService, ApiValidatorService>();
                services.AddSingleton<IExchangeHttpClient, BinanceClient>();
                services.AddSingleton<IFileSystem, FileSystem>();
                services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
                services.AddSingleton<ILogger, Logger>();
                services.AddSingleton<IMarketTradeHandler, MarketTradeHandler>();
                //services.AddSingleton<IPriceRetriever, PriceRetriever>();
                //services.AddSingleton<ITechnicalIndicatorsCalculator, TechnicalIndicatorsCalculator>();
                //services.AddSingleton<ITradeAction, TradeAction>();
                services.AddSingleton<IConfiguration>(config);
            });

        using IHost host = builder.Build();

        await RunBot(host.Services, "My singleton for BinanceBot is running...");

        Console.WriteLine();

        await host.RunAsync();

        Console.ReadLine();
    }
}