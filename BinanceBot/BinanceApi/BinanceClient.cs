using Newtonsoft.Json;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using BinanceBot.Model;

namespace BinanceBot.BinanceApi
{
    public class BinanceClient : IBinanceClient
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly bool _testApi;
        private static readonly string _baseEndpoint = "https://api.binance.com";
        private string _apiKey = string.Empty;
        private string _apiSecret = string.Empty;

        public BinanceClient(IConfiguration config, bool testApi)
        {
            _apiKey = config["AppSettings:Binance:ApiKey"] ?? string.Empty;
            _apiSecret = config["AppSettings:Binance:ApiSecret"] ?? string.Empty;

            _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", _apiKey);

            _testApi = testApi;
        }

        ~BinanceClient()
        {
            _client.Dispose();
        }

        public async Task<List<List<object>>> GetKLinesBySymbolAsync(string symbol, string interval, string limit)
        {
            var klinesEndpoint = $"{_baseEndpoint}/api/v3/klines?symbol={symbol}&interval={interval}&limit={limit}";
            var klinesResponse = await _client.GetStringAsync(klinesEndpoint);
            var klines = JsonConvert.DeserializeObject<List<List<object>>>(klinesResponse);

            return klines;
        }

        public async Task<Currency> GetPriceBySymbolAsync(string symbol)
        {
            var priceEndpoint = $"{_baseEndpoint}/api/v3/ticker/price?symbol={symbol}";
            var priceResponse = await _client.GetStringAsync(priceEndpoint);
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

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Order>>(responseContent);
        }

        public async Task<List<decimal>> RecuperePrixRecent(string symbol, string interval, int periode)
        {
            var url = $"{_baseEndpoint}/api/v3/klines?symbol={symbol}&interval={interval}&limit={periode}";

            try
            {
                var response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var klines = JsonConvert.DeserializeObject<List<List<object>>>(responseBody);

                var closingPrices = new List<decimal>();
                foreach (var kline in klines)
                {
                    if (!decimal.TryParse(kline[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var ret))
                        throw new InvalidCastException($"it cannot be converted to decimal for klines {JsonConvert.SerializeObject(kline)}");
                    closingPrices.Add(ret); // closing index is 4
                }

                return closingPrices;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Erreur dans la requête HTTP: {e.Message}");
                return new List<decimal>();
            }
        }

        public async Task<string> PlaceOrder(string symbol, decimal quantity, decimal price, string side)
        {
            var endpoint = $"{_baseEndpoint}/api/v3/order{(_testApi ? "/test" : "")}";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var quantitystr = quantity.ToString("0.######", CultureInfo.InvariantCulture);
            var pricestr = price.ToString("0.######", CultureInfo.InvariantCulture);
            var queryString = $"symbol={symbol}&side={side}&type=LIMIT&timeInForce=GTC&quantity={quantitystr}&price={pricestr}&timestamp={timestamp}";
            var signature = Sign(queryString, _apiSecret);

            var finalUrl = $"{endpoint}?{queryString}&signature={signature}";

            using var request = new HttpRequestMessage(HttpMethod.Post, finalUrl);

            var response = await _client.SendAsync(request);
            //response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsStringAsync();

            return res;
        }

        public async Task<decimal> GetBNBBalance()
        {
            var endpoint = $"{_baseEndpoint}/api/v3/account";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var queryString = $"timestamp={timestamp}";
            var signature = Sign(queryString, _apiSecret);

            var finalUrl = $"{endpoint}?{queryString}&signature={signature}";

            using var request = new HttpRequestMessage(HttpMethod.Get, finalUrl);

            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var jObject = JObject.Parse(responseBody);
            var balances = jObject["balances"];

            foreach (var balance in balances)
            {
                if (balance["asset"].ToString() == "USDT")
                {
                    return decimal.Parse(balance["free"].ToString());
                }
            }

            return 0m; // Retourne 0 si aucun solde BNB n'est trouvé
        }

        private static string Sign(string data, string secret)
        {
            using var hmacSha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
