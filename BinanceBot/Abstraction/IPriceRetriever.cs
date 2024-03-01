using TradingCalculation.Strategy;

namespace BinanceBot.Abstraction;

public interface IPriceRetriever
{
    Task HandleDiscountAsync(TradingStrategy tradingStrategy);
    List<decimal> GetClosingPrices(List<List<object>> klines);
}