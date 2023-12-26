namespace BinanceBot.Abstraction
{
    internal interface IMarketTradeHandler
    {
        Task TradeOnLimit();
        Task WaitBuy();
        Task WaitSell();
    }
}
