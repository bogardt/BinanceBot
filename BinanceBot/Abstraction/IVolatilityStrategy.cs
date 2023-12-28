using BinanceBot.Core;

namespace BinanceBot.Abstraction
{
    public interface IVolatilityStrategy
    {
        decimal CalculateVolatility(List<List<object>> klines);
        decimal DetermineLossStrategy(decimal volatility, TradingConfig tradingConfig);
    }
}