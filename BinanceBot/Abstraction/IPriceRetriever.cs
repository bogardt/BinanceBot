namespace BinanceBot.Abstraction
{
    public interface IPriceRetriever
    {
        List<decimal> GetRecentPrices(List<List<object>> klines);
    }
}