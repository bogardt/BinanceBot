namespace BinanceBot.Abstraction
{
    internal interface IMarketTradeHandler
    {
        Task TradeOnLimitAsync();
        Task WaitBuyAsync();
        Task WaitSellAsync();
    }
}
