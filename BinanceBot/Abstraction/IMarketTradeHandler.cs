namespace BinanceBot.Abstraction;

public interface IMarketTradeHandler
{
    Task TradeOnLimitAsync();
}
