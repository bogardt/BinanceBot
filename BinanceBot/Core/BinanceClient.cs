using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using BinanceBot.Model;
using BinanceBot.Abstraction;

namespace BinanceBot.Core;

public class BinanceClient(
    IHttpClientWrapper httpClientWrapper,
    IConfiguration config,
    ILogger logger) : IBinanceClient
{
    private readonly IHttpClientWrapper _httpClientWrapper = httpClientWrapper;
    private readonly ILogger _logger = logger;
    private readonly string _apiSecret = config["AppSettings:Binance:ApiSecret"] ?? string.Empty;
    private static readonly string _baseEndpoint = "https://api.binance.com";

    public async Task<Account> GetAccountInfosAsync()
    {
        var endpoint = $"{_baseEndpoint}/api/v3/account";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var queryString = $"timestamp={timestamp}";
        var signature = Sign(queryString, _apiSecret);
        queryString += $"&signature={signature}";
        var requestUrl = $"{endpoint}?{queryString}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        var response = await _httpClientWrapper.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(res))
            throw new JsonReaderException($"Unable to get account infos");

        var account = JsonConvert.DeserializeObject<Account>(res) ??
            throw new JsonReaderException($"Unable to get account infos {res}");

        if (account.Code != null && account.Message != null)
            throw new Exception($"Unable to get account infos {res}");

        return account;
    }

    public async Task<Commission> GetCommissionBySymbolAsync(string symbol)
    {
        var endpoint = $"{_baseEndpoint}/api/v3/account/commission";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var queryString = $"symbol={symbol}&timestamp={timestamp}";
        var signature = Sign(queryString, _apiSecret);
        queryString += $"&signature={signature}";
        var requestUrl = $"{endpoint}?{queryString}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        var response = await _httpClientWrapper.SendAsync(request);
        //response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(res))
            throw new JsonReaderException($"Unable to get wallet for {symbol} : {res}");

        var commission = JsonConvert.DeserializeObject<Commission>(res) ??
            throw new JsonReaderException($"Unable to get wallet for {symbol} : {res}");

        if (commission.Code != null && commission.Message != null)
            throw new Exception($"Unable to get wallet for {symbol} : {res}");

        return commission;
    }

    public async Task<List<List<object>>> GetKLinesBySymbolAsync(string symbol, string interval, string limit)
    {
        var klinesEndpoint = $"{_baseEndpoint}/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
        var res = await _httpClientWrapper.GetStringAsync(klinesEndpoint);
        if (string.IsNullOrEmpty(res))
            throw new JsonReaderException($"Unable to get klines for {symbol} : {res}");

        var klines = JsonConvert.DeserializeObject<List<List<object>>>(res) ??
            throw new JsonReaderException($"Unable to get klines for {symbol} : {res}");

        return klines;
    }

    public async Task<Currency> GetPriceBySymbolAsync(string symbol)
    {
        var priceEndpoint = $"{_baseEndpoint}/api/v3/ticker/price?symbol={symbol}";
        var res = await _httpClientWrapper.GetStringAsync(priceEndpoint);
        if (string.IsNullOrEmpty(res))
            throw new JsonReaderException($"Unable to get price for {symbol} : {res}");

        var currency = JsonConvert.DeserializeObject<Currency>(res) ??
            throw new JsonReaderException($"Unable to get price for {symbol} : {res}");

        if (currency.Code != null && currency.Message != null)
            throw new Exception($"Unable to get price for {symbol} : {res}");

        return currency;
    }

    public async Task<List<Order>> GetOpenOrdersAsync(string symbol)
    {
        var endpoint = $"{_baseEndpoint}/api/v3/openOrders";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var queryString = $"symbol={symbol}&timestamp={timestamp}";
        var signature = Sign(queryString, _apiSecret);
        queryString += $"&signature={signature}";
        var requestUrl = $"{endpoint}?{queryString}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        var response = await _httpClientWrapper.SendAsync(request);
        //response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(res))
            throw new JsonReaderException($"Unable to get orders for {symbol} : {res}");

        var orders = JsonConvert.DeserializeObject<List<Order>>(res) ??
            throw new JsonReaderException($"Unable to get orders for {symbol} : {res}");

        return orders;
    }

    public async Task<TestOrder> PlaceTestOrderAsync(string symbol, decimal quantity, decimal price, string side)
    {
        var endpoint = $"{_baseEndpoint}/api/v3/order/test";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var quantitystr = quantity.ToString("0.######", CultureInfo.InvariantCulture);
        var pricestr = price.ToString("0.######", CultureInfo.InvariantCulture);
        var queryString = $"symbol={symbol}&side={side}&type=LIMIT&timeInForce=GTC&quantity={quantitystr}" +
            $"&price={pricestr}&timestamp={timestamp}&computeCommissionRates=true";
        var signature = Sign(queryString, _apiSecret);

        var finalUrl = $"{endpoint}?{queryString}&signature={signature}";

        using var request = new HttpRequestMessage(HttpMethod.Post, finalUrl);

        var response = await _httpClientWrapper.SendAsync(request);
        //response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(res))
            throw new JsonReaderException($"Unable to place test order for {symbol} : {res}");

        var order = JsonConvert.DeserializeObject<TestOrder>(res) ??
            throw new JsonReaderException($"Unable to place test order for {symbol} : {res}");

        if (order.Code != null && order.Message != null)
            throw new Exception($"Unable to place test order for {symbol} : {res}");

        return order;
    }

    public async Task<Order> PlaceOrderAsync(string symbol, decimal quantity, decimal price, string side)
    {
        var endpoint = $"{_baseEndpoint}/api/v3/order";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var quantitystr = quantity.ToString("0.######", CultureInfo.InvariantCulture);
        var pricestr = price.ToString("0.######", CultureInfo.InvariantCulture);
        var queryString = $"symbol={symbol}&side={side}&type=LIMIT&timeInForce=GTC&quantity={quantitystr}" +
            $"&price={pricestr}&timestamp={timestamp}&computeCommissionRates=true";
        var signature = Sign(queryString, _apiSecret);

        var finalUrl = $"{endpoint}?{queryString}&signature={signature}";

        using var request = new HttpRequestMessage(HttpMethod.Post, finalUrl);

        var response = await _httpClientWrapper.SendAsync(request);
        //response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(res))
            throw new JsonReaderException($"Unable to place order for {symbol} : {res}");

        var order = JsonConvert.DeserializeObject<Order>(res) ??
            throw new JsonReaderException($"Unable to place order for {symbol} : {res}");

        if (order.Code != null && order.Message != null)
            throw new Exception($"Unable to place order for {symbol} : {res}");

        return order;
    }

    private static string Sign(string data, string secret)
    {
        using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}
