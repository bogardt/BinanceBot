using BinanceBot.Strategy;

namespace BinanceBot.Abstraction
{
    public interface IVolatilityStrategy
    {
        decimal CalculateVolatility(List<List<object>> klines);
        decimal DetermineLossStrategy(decimal cryptoPurchasePrice, decimal volatility);
    }
}