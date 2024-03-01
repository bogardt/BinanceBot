using BinanceBot.Abstraction;
using Newtonsoft.Json;
using TradingCalculation.Strategy;

namespace BinanceTradeOrderPlanner;

public class MarketTradeOrderPlanner(IExchangeHttpClient binanceClient,
    ITradeAction tradeAction,
    ILogger logger) : IMarketTradeHandler
{
    private readonly ILogger _logger = logger;
    private readonly TradingStrategy _tradingStrategy = new()
    {
        TargetProfit = 100m,
        Quantity = 200m,
        Period = 600,
        Interval = "1s",
        Symbol = "SOLUSDT"
    };

    public async Task TradeOnLimitAsync()
    {
        try
        {
            //var orderBuyResponse = await binanceClient.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, 132m, "BUY");
            //_logger.WriteLog($"buy : {JsonConvert.SerializeObject(orderBuyResponse, Formatting.Indented)}");
            //await tradeAction.WaitBuyAsync(_tradingStrategy.Symbol);

            //var orderSellResponse = await binanceClient.PlaceOrderAsync(_tradingStrategy.Symbol, _tradingStrategy.Quantity, 133m, "SELL");
            //_logger.WriteLog($"sell : {JsonConvert.SerializeObject(orderSellResponse, Formatting.Indented)}");
            //await tradeAction.WaitSellAsync(_tradingStrategy.Symbol);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}", ex);
            throw;
        }
    }
}
