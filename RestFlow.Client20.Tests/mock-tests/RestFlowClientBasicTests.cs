using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using RestFlow.Client20;
using RestFlow.Client20.Models;
using Xunit;

namespace RestFlow.Client20.Tests
{
    public class RestFlowClientBasicTests
    {
        [Fact]
        public void Constructor_WithoutHttpClient_ShouldCreateDefaultInstance()
        {
            // Act
            var client = new RestFlowClient();

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithCustomHttpClient_ShouldUseProvidedInstance()
        {
            // Arrange
            var httpClient = new HttpClient();

            // Act
            var client = new RestFlowClient(httpClient);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void WithBaseUrl_ShouldReturnClientInstance()
        {
            // Arrange
            var client = new RestFlowClient();

            // Act
            var result = client.WithBaseUrl("https://api.example.com");

            // Assert
            Assert.Same(client, result);
        }

        [Fact]
        public void WithBasicAuth_ShouldReturnClientInstance()
        {
            // Arrange
            var client = new RestFlowClient();

            // Act
            var result = client.WithBasicAuth("user", "pass");

            // Assert
            Assert.Same(client, result);
        }

        [Fact]
        public void WithBearerToken_ShouldReturnClientInstance()
        {
            // Arrange
            var client = new RestFlowClient();

            // Act
            var result = client.WithBearerToken("token123");

            // Assert
            Assert.Same(client, result);
        }

        [Fact]
        public void WithApiKey_ShouldReturnClientInstance()
        {
            // Arrange
            var client = new RestFlowClient();

            // Act
            var result = client.WithApiKey("X-API-Key", "key123", ApiKeyLocation.Header);

            // Assert
            Assert.Same(client, result);
        }

        [Fact]
        public async Task GetAsync_WithSuccessResponse_ShouldReturnDeserializedObject()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":1,\"name\":\"Test\"}")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com");

            // Act
            var result = await client.GetAsync<TestModel>("/test");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task PostAsync_WithSuccessResponse_ShouldReturnDeserializedObject()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent("{\"id\":2,\"name\":\"Created\"}")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com");

            // Act
            var result = await client.PostAsync<TestModel>("/test", new { name = "New Item" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("Created", result.Name);
        }

        [Fact]
        public async Task GetAsync_WithErrorResponse_ShouldThrowApiException()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Not Found")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<TestModel>("/test"));
            
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        }

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ShouldComplete()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com");

            // Act
            await client.DeleteAsync("/test/1");

            // Assert - no exception thrown
            mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task PutAsync_WithSuccessResponse_ShouldReturnDeserializedObject()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":1,\"name\":\"Updated\"}")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com");

            // Act
            var result = await client.PutAsync<TestModel>("/test/1", new { name = "Updated" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Updated", result.Name);
        }

        [Fact]
        public async Task PatchAsync_WithSuccessResponse_ShouldReturnDeserializedObject()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method.Method == "PATCH"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\":1,\"name\":\"Patched\"}")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com");

            // Act
            var result = await client.PatchAsync<TestModel>("/test/1", new { name = "Patched" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Patched", result.Name);
        }

        private class TestModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
