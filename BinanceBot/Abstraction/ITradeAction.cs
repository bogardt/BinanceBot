using BinanceBot.Core;

namespace BinanceBot.Abstraction
{
    public interface ITradeAction
    {
        Task Buy(TradingConfig tradingConfig, decimal currentCurrencyPrice, decimal volatility, string symbol);
        Task<(decimal, bool)> Sell(TradingConfig tradingConfig, decimal currentCurrencyPrice, decimal volatility, string symbol);
        Task WaitBuyAsync(string symbol);
        Task WaitSellAsync(string symbol);
    }
}