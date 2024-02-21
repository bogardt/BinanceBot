using BinanceBot.Strategy;

namespace BinanceBot.Abstraction;

public interface IPriceRetriever
{
    Task HandleDiscountAsync(TradingStrategy tradingStrategy);
    IEnumerable<decimal> GetClosingPrices(IEnumerable<IEnumerable<object>> klines);
    decimal CalculateMinimumSellingPrice(decimal cryptoPurchasePrice, decimal quantity, decimal feePercentage, decimal discount, decimal targetProfit);
}