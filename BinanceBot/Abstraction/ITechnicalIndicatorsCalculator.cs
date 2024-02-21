namespace BinanceBot.Abstraction;

public interface ITechnicalIndicatorsCalculator
{
    decimal CalculateMovingAverage(IEnumerable<decimal> closingPrices, int periode);
    decimal CalculateRSI(IEnumerable<decimal> closingPrices, int periode);
    decimal CalculateVolatility(IEnumerable<decimal> closingPrices);
    decimal DetermineLossStrategy(decimal cryptoPurchasePrice, decimal volatility);
}