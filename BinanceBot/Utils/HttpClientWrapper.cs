using BinanceBot.Abstraction;
using Microsoft.Extensions.Configuration;

namespace BinanceBot.Utils
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient = new();

        public HttpClientWrapper(IConfiguration config)
        {
            _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", config["AppSettings:Binance:ApiKey"]);
        }
        ~HttpClientWrapper()
        {
            _httpClient.Dispose();
        }

        public Task<string> GetStringAsync(string uri)
        {
            return _httpClient.GetStringAsync(uri);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return _httpClient.SendAsync(request, cancellationToken);
        }
    }
}
