using BinanceBot.Utils;
using Moq.Protected;
using Moq;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace BinanceBot.Tests
{
    [TestClass]
    public class HttpClientWrapperTests
    {
        private readonly IConfiguration _config;

        public HttpClientWrapperTests()
        {
            var appSettings = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("AppSettings:Binance:ApiKey", "***"),
                new KeyValuePair<string, string?>("AppSettings:Binance:ApiSecret", "***")
            };
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();
        }

        [TestMethod]
        public async Task GetStringAsync_ReturnsStringSuccessfully()
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
        public async Task SendAsync_SendsRequestSuccessfully()
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
    }
}
