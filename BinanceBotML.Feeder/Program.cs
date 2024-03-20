using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BinanceBot.Core;
using BinanceBot.Utils;
using BinanceBot.BinanceApi;
using Microsoft.Extensions.Configuration;
using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Validation;
using FluentValidation;
using BinanceBot.BinanceApi.Validation.Validator;
using TradingCalculation;
using BinanceBotML.Feeder;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var helper = new Helper(new FileSystem());
        var solutionPath = helper.GetSolutionPath();

        static async Task RunScriptCsvFeeder(IServiceProvider services, string scope)
        {
            Console.WriteLine($"{scope}...");

            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var feeder = provider.GetRequiredService<IFeeder>();

            var helper = new Helper(new FileSystem());
            var solutionPath = helper.GetSolutionPath();

            await feeder.Run10Min(solutionPath + "\\test.csv");
        }

        var config = new ConfigurationBuilder()
            .SetBasePath(solutionPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddValidatorsFromAssemblyContaining<AccountValidator>();
                services.AddSingleton<IFeeder, FeedCsv>();
                services.AddSingleton<IApiValidatorService, ApiValidatorService>();
                services.AddSingleton<IExchangeHttpClient, BinanceClient>();
                services.AddSingleton<IFileSystem, FileSystem>();
                services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
                services.AddSingleton<ILogger, Logger>();
                services.AddSingleton<IMarketTradeHandler, MarketTradeHandler>();
                services.AddSingleton<IPriceRetriever, PriceRetriever>();
                services.AddSingleton<ITechnicalIndicatorsCalculator, TechnicalIndicatorsCalculator>();
                services.AddSingleton<ITradeAction, TradeAction>();
                services.AddSingleton<IConfiguration>(config);
            });

        using IHost host = builder.Build();

        await RunScriptCsvFeeder(host.Services, "Running script csv feeder...");

        Console.WriteLine();

        await host.RunAsync();

        Console.ReadLine();
    }
}