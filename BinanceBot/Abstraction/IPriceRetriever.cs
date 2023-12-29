using BinanceBot.Strategy;

namespace BinanceBot.Abstraction
{
    public interface IPriceRetriever
    {
        Task HandleDiscount(TradingStrategy tradingStrategy);
        List<decimal> GetClosingPrices(List<List<object>> klines);
        decimal CalculateMinimumSellingPrice(decimal cryptoPurchasePrice, decimal quantity, decimal feePercentage, decimal discount, decimal targetProfit);
    }
}