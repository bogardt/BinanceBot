namespace BinanceBot.Abstraction;

public interface ITechnicalIndicatorsCalculator
{
    decimal CalculateMovingAverage(List<decimal> closingPrices, int periode);
    decimal CalculateRSI(List<decimal> closingPrices, int periode);
    decimal CalculateVolatility(List<decimal> closingPrices);
    decimal DetermineLossStrategy(decimal cryptoPurchasePrice, decimal volatility);
}