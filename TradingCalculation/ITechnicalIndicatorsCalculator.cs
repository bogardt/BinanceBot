namespace TradingCalculation;

public interface ITechnicalIndicatorsCalculator
{
    decimal CalculateMovingAverage(IEnumerable<decimal> closingPrices, int periode);
    decimal CalculateRSI(IEnumerable<decimal> closingPrices, int periode);
    decimal CalculateVolatility(IEnumerable<decimal> closingPrices);
    decimal DetermineLossStrategy(decimal cryptoPurchasePrice, decimal volatility);
    decimal CalculateProfit(decimal cryptoPurchasePrice, decimal cryptoSellingPrice, decimal quantity, decimal feePercentage, decimal discount);
    decimal CalculateMinimumSellingPrice(decimal cryptoPurchasePrice, decimal quantity, decimal feePercentage, decimal discount, decimal targetProfit);
}