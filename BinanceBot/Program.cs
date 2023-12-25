using BinanceBot.BinanceApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using BinanceBot.Core;
using BinanceBot.Logger;
using Microsoft.Extensions.Configuration;

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

        string solutionPath = GetSolutionPath();

        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(solutionPath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IBinanceClient, BinanceClient>((s) => new BinanceClient(config, true));
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

    public static string GetSolutionPath()
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        var directoryInfo = new DirectoryInfo(currentDirectory);

        while (directoryInfo != null && !directoryInfo.GetFiles("*.sln").Any())
        {
            directoryInfo = directoryInfo.Parent;
        }

        var solutionPath = directoryInfo?.FullName;

        return solutionPath;
    }

}