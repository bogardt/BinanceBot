namespace TradingCalculation;

public interface ITechnicalIndicatorsCalculator
{
    decimal CalculateMovingAverage(IEnumerable<decimal> closingPrices, int period);
    decimal CalculateRSI(IEnumerable<decimal> closingPrices, int period);
    decimal CalculateVolatility(IEnumerable<decimal> closingPrices);
    decimal DetermineLossStrategy(decimal cryptoPurchasePrice, decimal volatility);
    decimal CalculateProfit(decimal cryptoPurchasePrice, decimal cryptoSellingPrice, decimal quantity, decimal feePercentage, decimal discount);
    decimal CalculateMinimumSellingPrice(decimal cryptoPurchasePrice, decimal quantity, decimal feePercentage, decimal discount, decimal targetProfit);
    bool IsTargetPriceAchievable(decimal targetPrice, List<decimal> closingPrices);
    decimal CalculateStandardDeviation(List<decimal> values, decimal mean);
}