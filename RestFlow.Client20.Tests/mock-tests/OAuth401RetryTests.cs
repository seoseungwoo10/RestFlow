using System;
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
    public class OAuth401RetryTests
    {
        [Fact]
        public async Task GetAsync_With401Response_ShouldRetryWithRefreshedToken()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int apiCallCount = 0;
            int tokenCallCount = 0;
            string currentToken = "initial_token";

            // Setup token endpoint
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    tokenCallCount++;
                    currentToken = $"token_{tokenCallCount}";
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent($"{{\"access_token\":\"{currentToken}\",\"expires_in\":3600}}")
                    };
                });

            // Setup API endpoint - first call returns 401, second call succeeds
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/api/data")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    apiCallCount++;
                    if (apiCallCount == 1)
                    {
                        // First call - return 401 (token expired on server side)
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.Unauthorized,
                            Content = new StringContent("Unauthorized")
                        };
                    }
                    // Second call - return success
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"id\":1,\"value\":\"success\"}")
                    };
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com")
                .WithOAuthClientCredentials(
                    "https://auth.example.com/token",
                    "client_id",
                    "client_secret",
                    options: new OAuthOptions
                    {
                        EnableAutoRetryOn401 = true
                    });

            // Act
            var result = await client.GetAsync<TestResponse>("/api/data");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("success", result.Value);
            Assert.Equal(2, apiCallCount); // API called twice (401 + retry)
            Assert.Equal(2, tokenCallCount); // Token refreshed twice (initial + force refresh)
        }

        [Fact]
        public async Task GetAsync_With401Response_WhenAutoRetryDisabled_ShouldThrowException()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();

            // Setup token endpoint
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\":\"test_token\",\"expires_in\":3600}")
                });

            // Setup API endpoint - always returns 401
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/api/data")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Unauthorized")
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com")
                .WithOAuthClientCredentials(
                    "https://auth.example.com/token",
                    "client_id",
                    "client_secret",
                    options: new OAuthOptions
                    {
                        EnableAutoRetryOn401 = false
                    });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<TestResponse>("/api/data"));
            
            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }

        [Fact]
        public async Task GetAsync_With401ThenAnother401_ShouldThrowExceptionAfterOneRetry()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int apiCallCount = 0;

            // Setup token endpoint
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\":\"test_token\",\"expires_in\":3600}")
                });

            // Setup API endpoint - always returns 401
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/api/data")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    apiCallCount++;
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        Content = new StringContent("Unauthorized")
                    };
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com")
                .WithOAuthClientCredentials(
                    "https://auth.example.com/token",
                    "client_id",
                    "client_secret",
                    options: new OAuthOptions
                    {
                        EnableAutoRetryOn401 = true
                    });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<TestResponse>("/api/data"));
            
            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
            Assert.Contains("after token refresh retry", exception.Message);
            Assert.Equal(2, apiCallCount); // Should only retry once
        }

        [Fact]
        public async Task PostAsync_With401Response_ShouldRetryWithRefreshedToken()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int apiCallCount = 0;

            // Setup token endpoint
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"access_token\":\"refreshed_token\",\"expires_in\":3600}")
                });

            // Setup API endpoint
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/api/data") && 
                        req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    apiCallCount++;
                    if (apiCallCount == 1)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.Unauthorized
                        };
                    }
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Content = new StringContent("{\"id\":2,\"value\":\"created\"}")
                    };
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com")
                .WithOAuthClientCredentials(
                    "https://auth.example.com/token",
                    "client_id",
                    "client_secret");

            // Act
            var result = await client.PostAsync<TestResponse>("/api/data", new { name = "test" });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Id);
            Assert.Equal("created", result.Value);
            Assert.Equal(2, apiCallCount);
        }

        [Fact]
        public async Task GetAsync_WithNonOAuthHandler_Should401WithoutRetry()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int apiCallCount = 0;

            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    apiCallCount++;
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Unauthorized,
                        Content = new StringContent("Unauthorized")
                    };
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var client = new RestFlowClient(httpClient)
                .WithBaseUrl("https://api.example.com")
                .WithBearerToken("static_token");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<TestResponse>("/api/data"));
            
            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
            Assert.Equal(1, apiCallCount); // No retry for non-OAuth handlers
        }

        private class TestResponse
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }
    }
}
