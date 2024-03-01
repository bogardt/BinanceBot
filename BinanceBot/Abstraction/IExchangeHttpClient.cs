using BinanceBot.BinanceApi.Model;

namespace BinanceBot.Abstraction;

public interface IExchangeHttpClient
{
    Task<List<List<object>>> GetKLinesBySymbolAsync(string symbol, string interval, string limit); // todo : remettre IEnum<IEnum<object>>
    Task<Currency> GetPriceBySymbolAsync(string symbol);
    Task<TestOrder> PlaceTestOrderAsync(string symbol, decimal quantity, decimal price, string side);
    Task<Order> PlaceOrderAsync(string symbol, decimal quantity, decimal price, string side);
    Task<IEnumerable<Order>> GetOpenOrdersAsync(string symbol);
    Task<Account> GetAccountInfosAsync();
    Task<Commission> GetCommissionBySymbolAsync(string symbol);
}
