﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BinanceBot.Core;
using BinanceBot.Utils;
using Microsoft.Extensions.Configuration;
using BinanceBot.Abstraction;

internal class Program
{
    private static async Task Main(string[] args)
    {
        static async Task RunBot(IServiceProvider services, string scope)
        {
            Console.WriteLine($"{scope}...");

            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var binanceBot = provider.GetRequiredService<IMarketTradeHandler>();

            await binanceBot.TradeOnLimitAsync();
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
                services.AddSingleton<IMarketTradeHandler, MarketTradeHandler>();
                services.AddSingleton<IBinanceClient, BinanceClient>();
                services.AddSingleton<ITradeAction, TradeAction>();
                services.AddSingleton<IFileSystem, FileSystem>();
                services.AddSingleton<IVolatilityStrategy, VolatilityStrategy>();
                services.AddSingleton<ITechnicalIndicatorsCalculator, TechnicalIndicatorsCalculator>();
                services.AddSingleton<IPriceRetriever, PriceRetriever>();
                services.AddSingleton<IMarketTradeHandler, MarketTradeHandler>();
                services.AddSingleton<ILogger, Logger>();
                services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
                services.AddSingleton<IConfiguration>(config);
            });

        using IHost host = builder.Build();

        await RunBot(host.Services, "My singleton for BinanceBot is running...");

        Console.WriteLine();

        await host.RunAsync();

        Console.ReadLine();
    }
}