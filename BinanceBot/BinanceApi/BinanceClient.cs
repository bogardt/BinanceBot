using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using BinanceBot.Abstraction;
using BinanceBot.BinanceApi.Model;

namespace BinanceBot.BinanceApi;

public class BinanceClient(
    IHttpClientWrapper httpClientWrapper,
    IConfiguration config,
    IApiValidatorService apiValidator,
    ILogger logger) : IBinanceClient
{
    private readonly IHttpClientWrapper _httpClientWrapper = httpClientWrapper;
    private readonly ILogger _logger = logger;
    private readonly string _apiSecret = config["AppSettings:Binance:ApiSecret"] ?? string.Empty;
    private static readonly string _baseEndpoint = "https://api.binance.com";

    public async Task<Account> GetAccountInfosAsync()
    {
        string requestUrl = GetUrl("account");
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        var response = await _httpClientWrapper.SendAsync(request);
        var account = await apiValidator.ValidateAsync<Account>(response);
        return account!;
    }

    public async Task<Commission> GetCommissionBySymbolAsync(string symbol)
    {
        string requestUrl = GetUrl("account/commission", symbol);
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        var response = await _httpClientWrapper.SendAsync(request);
        var commission = await apiValidator.ValidateAsync<Commission>(response);
        return commission!;
    }

    public async Task<IEnumerable<Order>> GetOpenOrdersAsync(string symbol)
    {
        string requestUrl = GetUrl("openOrders", symbol);
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        var response = await _httpClientWrapper.SendAsync(request);
        var orders = await apiValidator.Validate1DResponse<Order>(response);
        return (IEnumerable<Order>)orders;
    }

    private string GetUrl(string method, string? symbol = null)
    {
        var endpoint = $"{_baseEndpoint}/api/v3/{method}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var queryString = (symbol is not null ? $"symbol={symbol}" : "") +
            $"&timestamp={timestamp}";
        var signature = Sign(queryString, _apiSecret);
        queryString += $"&signature={signature}";
        var requestUrl = $"{endpoint}?{queryString}";
        return requestUrl;
    }

    public async Task<IEnumerable<IEnumerable<object>>> GetKLinesBySymbolAsync(string symbol, string interval, string limit)
    {
        var klinesEndpoint = $"{_baseEndpoint}/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
        var response = await _httpClientWrapper.GetAsync(klinesEndpoint);
        var klines = await apiValidator.Validate2DMatriceResponse(response);
        return klines!;
    }

    public async Task<Currency> GetPriceBySymbolAsync(string symbol)
    {
        var priceEndpoint = $"{_baseEndpoint}/api/v3/ticker/price?symbol={symbol}";
        var response = await _httpClientWrapper.GetAsync(priceEndpoint);
        var currency = await apiValidator.ValidateAsync<Currency>(response);
        return currency;
    }

    public async Task<TestOrder> PlaceTestOrderAsync(string symbol, decimal quantity, decimal price, string side)
    {
        string finalUrl = CreateOrderUrl("order/test", symbol, quantity, price, side);
        using var request = new HttpRequestMessage(HttpMethod.Post, finalUrl);
        var response = await _httpClientWrapper.SendAsync(request);
        var testOrder = await apiValidator.ValidateAsync<TestOrder>(response);
        return testOrder;
    }

    public async Task<Order> PlaceOrderAsync(string symbol, decimal quantity, decimal price, string side)
    {
        string finalUrl = CreateOrderUrl("order", symbol, quantity, price, side);
        using var request = new HttpRequestMessage(HttpMethod.Post, finalUrl);
        var response = await _httpClientWrapper.SendAsync(request);
        var order = await apiValidator.ValidateAsync<Order>(response);
        return order;
    }

    private string CreateOrderUrl(string method, string symbol, decimal quantity, decimal price, string side)
    {
        var endpoint = $"{_baseEndpoint}/api/v3/{method}";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var quantitystr = quantity.ToString("0.######", CultureInfo.InvariantCulture);
        var pricestr = price.ToString("0.######", CultureInfo.InvariantCulture);
        var queryString = $"symbol={symbol}&side={side}&type=LIMIT&timeInForce=GTC&quantity={quantitystr}" +
            $"&price={pricestr}&timestamp={timestamp}&computeCommissionRates=true";
        var signature = Sign(queryString, _apiSecret);
        var finalUrl = $"{endpoint}?{queryString}&signature={signature}";
        return finalUrl;
    }

    private static string Sign(string data, string secret)
    {
        using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
