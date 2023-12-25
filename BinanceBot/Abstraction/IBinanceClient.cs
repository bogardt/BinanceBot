using BinanceBot.Model;

namespace BinanceBot.Abstraction
{
    public interface IBinanceClient
    {
        Task<List<List<object>>> GetKLinesBySymbolAsync(string symbol, string interval, string limit);
        Task<Currency> GetPriceBySymbolAsync(string symbol);
        Task<List<decimal>> RecuperePrixRecent(string symbol, string interval, int periode);
        Task<string> PlaceOrder(string symbol, decimal quantity, decimal price, string side);
        Task<decimal> GetBNBBalance();
        Task<List<Order>> GetOpenOrdersAsync(string symbol);
    }
}
