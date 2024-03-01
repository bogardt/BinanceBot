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

    public async Task<HttpResponseMessage> GetAsync(string uri) =>
        await _httpClient.GetAsync(uri);

    public async Task<string> GetStringAsync(string uri)
    {
        var httpResponseMessage = await GetAsync(uri);
        return await httpResponseMessage!.Content.ReadAsStringAsync();
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default) =>
        await _httpClient.SendAsync(request, cancellationToken);

    public async Task<string> SendStringAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        var httpResponseMessage = await SendAsync(request, cancellationToken);
        return await httpResponseMessage!.Content.ReadAsStringAsync(cancellationToken);
    }
}
