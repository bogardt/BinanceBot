using BinanceBot.Abstraction;
using Microsoft.Extensions.Configuration;

namespace BinanceBot.Utils;

public class HttpClientWrapper : IHttpClientWrapper, IDisposable
{
    private readonly HttpClient _httpClient = new();

    public HttpClientWrapper(IConfiguration config) => _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", config["AppSettings:Binance:ApiKey"]);

    public HttpClientWrapper(IConfiguration config, HttpClient client)
    {
        _httpClient = client;
        _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", config["AppSettings:Binance:ApiKey"]);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public async Task<string> GetStringAsync(string uri)
    {
        return await _httpClient.GetStringAsync(uri);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        return await _httpClient.SendAsync(request, cancellationToken);
    }
}
