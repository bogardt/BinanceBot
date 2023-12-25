using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BinanceBot.Core;
using BinanceBot.Utils;
using Microsoft.Extensions.Configuration;
using BinanceBot.Abstraction;

internal class Program
{
    private static async Task Main(string[] args)
    {
        static async Task ExemplifyDisposableScoping(IServiceProvider services, string scope)
        {
            Console.WriteLine($"{scope}...");

            using IServiceScope serviceScope = services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var binanceBot = provider.GetRequiredService<IMarketTradeHandler>();

            await binanceBot.TradeLimit();
        }

        var solutionPath = Helper.GetSolutionPath();

        var config = new ConfigurationBuilder()
            .SetBasePath(solutionPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // todo : change testApi to false if you want to trade with real money
                services.AddSingleton<IBinanceClient, BinanceClient>((s) => new BinanceClient(config, testApi: true));
                services.AddSingleton<IMarketTradeHandler, MarketTradeHandler>();
                services.AddSingleton<ILogger, Logger>();
                services.AddSingleton<IConfiguration>(config);
            });

        using IHost host = builder.Build();

        await ExemplifyDisposableScoping(host.Services, "My singleton for BinanceBot is running...");

        Console.WriteLine();

        await host.RunAsync();

        Console.ReadLine();
    }
}