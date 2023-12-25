namespace BinanceBot.Abstraction
{
    internal interface IMarketTradeHandler
    {
        Task TradeLimit();
        Task WaitBuy();
        Task WaitSell();

    }
}
