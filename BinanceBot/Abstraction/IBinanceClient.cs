using BinanceBot.Model;

namespace BinanceBot.Abstraction
{
    public interface IBinanceClient
    {
        Task<List<List<object>>> GetKLinesBySymbolAsync(string symbol, string interval, string limit);
        Task<Currency> GetPriceBySymbolAsync(string symbol);
        Task<TestOrder> PlaceTestOrderAsync(string symbol, decimal quantity, decimal price, string side);
        //Task<Order> PlaceOrderAsync(string symbol, decimal quantity, decimal price, string side);
        Task<List<Order>> GetOpenOrdersAsync(string symbol);
        Task<Account> GetAccountInfosAsync();
        Task<Commission> GetCommissionBySymbolAsync(string symbol);

    }
}
