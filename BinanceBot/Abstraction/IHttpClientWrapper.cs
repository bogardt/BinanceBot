namespace BinanceBot.Abstraction
{
    public interface IHttpClientWrapper
    {
        Task<string> GetStringAsync(string uri);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default);
    }
}
