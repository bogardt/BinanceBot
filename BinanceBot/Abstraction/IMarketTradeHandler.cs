namespace BinanceBot.Abstraction
{
    internal interface IMarketTradeHandler
    {
        Task TradeOnLimitAsync();
    }
}
