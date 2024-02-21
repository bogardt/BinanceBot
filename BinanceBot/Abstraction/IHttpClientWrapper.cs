namespace BinanceBot.Abstraction;

public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> GetAsync(string uri);
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
}
