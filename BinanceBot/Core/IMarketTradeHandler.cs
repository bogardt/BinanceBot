namespace BinanceBot.Core
{
    internal interface IMarketTradeHandler
    {
        Task TradeLimit();
        Task WaitBuy();
        Task WaitSell();

    }
}
