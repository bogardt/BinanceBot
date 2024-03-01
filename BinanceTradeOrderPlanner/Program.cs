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
using BinanceTradeOrderPlanner;
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

            var marketTradeOrderPlanner = provider.GetRequiredService<IMarketTradeHandler>();

            await marketTradeOrderPlanner.TradeOnLimitAsync();
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

                services.AddSingleton<IApiValidatorService, ApiValidatorService>();
                services.AddSingleton<IExchangeHttpClient, BinanceClient>();
                services.AddSingleton<IFileSystem, FileSystem>();
                services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
                services.AddSingleton<ILogger, Logger>();
                services.AddSingleton<IMarketTradeHandler, MarketTradeOrderPlanner>();
                services.AddSingleton<IPriceRetriever, PriceRetriever>();
                services.AddSingleton<ITechnicalIndicatorsCalculator, TechnicalIndicatorsCalculator>();
                services.AddSingleton<ITradeAction, TradeAction>();
                services.AddSingleton<IConfiguration>(config);
            });

        using IHost host = builder.Build();

        await RunBot(host.Services, "My singleton for BinanceBot is running...");

        Console.WriteLine();

        await host.RunAsync();

        Console.ReadLine();
    }
}