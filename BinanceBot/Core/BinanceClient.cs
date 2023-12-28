using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using BinanceBot.Model;
using BinanceBot.Abstraction;

namespace BinanceBot.Core
{
    public class BinanceClient : IBinanceClient
    {
        private readonly IHttpClientWrapper _httpClientWrapper;
        private static readonly string _baseEndpoint = "https://api.binance.com";
        private readonly string _apiSecret = string.Empty;
        private readonly bool _testApi;

        public BinanceClient(IHttpClientWrapper httpClientWrapper,
            ILogger logger,
            IConfiguration config)
        {
            _testApi = true;
            _apiSecret = config["AppSettings:Binance:ApiSecret"] ?? string.Empty;
            _httpClientWrapper = httpClientWrapper;
            logger.WriteLog($"start in {(_testApi ? "test" : "real")} mode.");
        }

        public async Task<List<List<object>>> GetKLinesBySymbolAsync(string symbol, string interval, string limit)
        {
            var klinesEndpoint = $"{_baseEndpoint}/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
            var klinesResponse = await _httpClientWrapper.GetStringAsync(klinesEndpoint);
            var klines = JsonConvert.DeserializeObject<List<List<object>>>(klinesResponse);

            return klines;
        }

        public async Task<Currency> GetPriceBySymbolAsync(string symbol)
        {
            var priceEndpoint = $"{_baseEndpoint}/api/v3/ticker/price?symbol={symbol}";
            var priceResponse = await _httpClientWrapper.GetStringAsync(priceEndpoint);
            var currency = JsonConvert.DeserializeObject<Currency>(priceResponse);

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
            string responseContent = await response.Content.ReadAsStringAsync();
            var orders = JsonConvert.DeserializeObject<List<Order>>(responseContent);

            return orders;
        }

        public async Task<string> PlaceOrderAsync(string symbol, decimal quantity, decimal price, string side)
        {
            var endpoint = $"{_baseEndpoint}/api/v3/order{(_testApi ? "/test" : "")}";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var quantitystr = quantity.ToString("0.######", CultureInfo.InvariantCulture);
            var pricestr = price.ToString("0.######", CultureInfo.InvariantCulture);
            var queryString = $"symbol={symbol}&side={side}&type=LIMIT&timeInForce=GTC&quantity={quantitystr}&price={pricestr}&timestamp={timestamp}";
            var signature = Sign(queryString, _apiSecret);

            var finalUrl = $"{endpoint}?{queryString}&signature={signature}";

            using var request = new HttpRequestMessage(HttpMethod.Post, finalUrl);

            var response = await _httpClientWrapper.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsStringAsync();

            return res;
        }

        private static string Sign(string data, string secret)
        {
            using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
