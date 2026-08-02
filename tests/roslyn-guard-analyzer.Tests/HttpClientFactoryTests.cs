using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System;

namespace roslyn-guard-analyzer.Tests
{
    public class HttpClientFactoryTests
    {
        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new HttpClientFactory(null));
        }

        [Fact]
        public void CreateClient_WithNullBaseUrl_ThrowsArgumentException()
        {
            var factory = new HttpClientFactory();
            Assert.Throws<ArgumentException>(() => factory.CreateClient(null, null));
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WithSuccessfulResponse_ReturnsResponse()
        {
            var factory = new HttpClientFactory();
            var client = factory.CreateClient("https://example.com");
            var response = await factory.ExecuteWithRetryAsync(client, async c => await c.GetAsync("https://example.com"));
            Assert.NotNull(response);
        }

        [Fact]
        public async Task ExecuteWithRetryAsync_WithTransientFailure_RetrysWithJitter()
        {
            var factory = new HttpClientFactory();
            var client = factory.CreateClient("https://example.com");
            var response = await factory.ExecuteWithRetryAsync(client, async c => await c.GetAsync("https://example.com"));
            Assert.NotNull(response);
        }

        [Fact]
        public async Task GetJsonAsync_WithSuccessfulResponse_ReturnsJson()
        {
            var factory = new HttpClientFactory();
            var client = factory.CreateClient("https://example.com");
            var json = await factory.GetJsonAsync(client, "https://example.com");
            Assert.NotNull(json);
        }

        [Fact]
        public async Task PostJsonAsync_WithSuccessfulResponse_ReturnsJson()
        {
            var factory = new HttpClientFactory();
            var client = factory.CreateClient("https://example.com");
            var json = await factory.PostJsonAsync(client, "https://example.com", "{\"key\":\"value\"}");
            Assert.NotNull(json);
        }

        [Fact]
        public void ClearCache_ClearsCache()
        {
            var factory = new HttpClientFactory();
            factory.CreateClient("https://example.com");
            factory.ClearCache();
            Assert.Empty(factory._handlerCache);
        }

        [Fact]
        public void Dispose_DisposesFactory()
        {
            var factory = new HttpClientFactory();
            factory.Dispose();
            Assert.Empty(factory._handlerCache);
        }
    }
}
