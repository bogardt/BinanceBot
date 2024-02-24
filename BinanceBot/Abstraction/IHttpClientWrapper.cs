namespace BinanceBot.Abstraction;

public interface IHttpClientWrapper
{
    Task<string> GetStringAsync(string uri);
    Task<string> SendStringAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    Task<HttpResponseMessage> GetAsync(string uri);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
