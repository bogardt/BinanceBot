using BinanceBot.Model;

namespace BinanceBot.Abstraction
{
    public interface IBinanceClient
    {
        Task<List<List<object>>> GetKLinesBySymbolAsync(string symbol, string interval, string limit);
        Task<Currency> GetPriceBySymbolAsync(string symbol);
        Task<string> PlaceOrder(string symbol, decimal quantity, decimal price, string side);
        Task<List<Order>> GetOpenOrdersAsync(string symbol);
    }
}
