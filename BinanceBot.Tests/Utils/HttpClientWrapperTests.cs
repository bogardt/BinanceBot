using BinanceBot.Utils;
using Moq.Protected;
using Moq;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace BinanceBot.Tests.Utils;

[TestClass]
public class HttpClientWrapperTests
{
    private readonly IConfiguration _config;

    public HttpClientWrapperTests()
    {
        var appSettings = new List<KeyValuePair<string, string?>>
        {
            new("AppSettings:Binance:ApiKey", "***"),
            new("AppSettings:Binance:ApiSecret", "***")
        };
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(appSettings)
            .Build();
    }

    [TestMethod]
    public async Task GetStringAsyncReturnsStringSuccessfully()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Test")
        };
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(fakeResponse);

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientWrapper = new HttpClientWrapper(_config, mockHttpClient);

        // Act
        var result = await httpClientWrapper.GetStringAsync("http://test.com");

        // Assert
        Assert.AreEqual("Test", result);
    }

    [TestMethod]
    public async Task SendAsyncSendsRequestSuccessfully()
    {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Success")
        };
        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(fakeResponse);

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        var httpClientWrapper = new HttpClientWrapper(_config, mockHttpClient);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var response = await httpClientWrapper.SendAsync(request);

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.AreEqual("Success", await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    public void HttpClientWrapperDisposeCalledExplicitly()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["AppSettings:Binance:ApiKey"]).Returns("test_key");

        var wrapper = new HttpClientWrapper(configurationMock.Object);

        // Act
        wrapper.Dispose();

        // Assert
        Assert.ThrowsException<ObjectDisposedException>(() =>
        {
            var task = wrapper.GetStringAsync("http://test.com");
            task.GetAwaiter().GetResult();
        });
    }

    public class TestableHttpClientWrapper : HttpClientWrapper, IDisposable
    {
        public bool Disposed { get; private set; } = false;

        public TestableHttpClientWrapper(IConfiguration config) : base(config)
        {
        }

        public new void Dispose()
        {
            base.Dispose();
            Disposed = true;
        }
    }

    [TestMethod]
    public void DisposeShouldBeCalled()
    {
        // Arrange
        var configurationMock = new Mock<IConfiguration>();
        var wrapper = new TestableHttpClientWrapper(configurationMock.Object);

        // Act
        wrapper.Dispose();

        // Assert
        Assert.IsTrue(wrapper.Disposed);
    }
}
