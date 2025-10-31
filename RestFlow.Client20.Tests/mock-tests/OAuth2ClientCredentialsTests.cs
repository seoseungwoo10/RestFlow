using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using RestFlow.Client20;
using RestFlow.Client20.Handlers.Authentication;
using RestFlow.Client20.Models;
using Xunit;

namespace RestFlow.Client20.Tests
{
    public class OAuth2ClientCredentialsTests
    {
        [Fact]
        public async Task ApplyAsync_WhenTokenExpired_ShouldRequestNewToken()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            
            // Mock token endpoint response
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

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var handler = new OAuth2ClientCredentialsHandler(
                "https://auth.example.com/token",
                "client_id",
                "client_secret",
                httpClient: httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            await handler.ApplyAsync(request);

            // Assert
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
            Assert.Equal("test_token", request.Headers.Authorization.Parameter);

            // Verify token endpoint was called
            mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.RequestUri.ToString().Contains("/token")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ApplyAsync_WhenTokenNotExpired_ShouldReuseToken()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int tokenRequestCount = 0;

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
                })
                .Callback(() => tokenRequestCount++);

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var handler = new OAuth2ClientCredentialsHandler(
                "https://auth.example.com/token",
                "client_id",
                "client_secret",
                httpClient: httpClient);

            // Act - Apply twice
            var request1 = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test1");
            await handler.ApplyAsync(request1);

            var request2 = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test2");
            await handler.ApplyAsync(request2);

            // Assert - Token should only be requested once
            Assert.Equal(1, tokenRequestCount);
            Assert.Equal("test_token", request1.Headers.Authorization.Parameter);
            Assert.Equal("test_token", request2.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task ApplyAsync_WithClockSkew_ShouldRefreshBeforeExpiration()
        {
            // Arrange
            var mockTimeProvider = new Mock<ITimeProvider>();
            var currentTime = DateTime.UtcNow;
            mockTimeProvider.Setup(tp => tp.UtcNow).Returns(currentTime);

            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int tokenRequestCount = 0;

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
                    Content = new StringContent("{\"access_token\":\"test_token\",\"expires_in\":180}")
                })
                .Callback(() => tokenRequestCount++);

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var options = new OAuthOptions
            {
                TimeProvider = mockTimeProvider.Object,
                ClockSkewSeconds = 120
            };

            var handler = new OAuth2ClientCredentialsHandler(
                "https://auth.example.com/token",
                "client_id",
                "client_secret",
                options: options,
                httpClient: httpClient);

            // First request - should get token
            var request1 = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test1");
            await handler.ApplyAsync(request1);
            Assert.Equal(1, tokenRequestCount);

            // Advance time to 61 seconds (within clock skew window)
            mockTimeProvider.Setup(tp => tp.UtcNow).Returns(currentTime.AddSeconds(61));

            // Second request - should refresh token due to clock skew
            var request2 = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test2");
            await handler.ApplyAsync(request2);

            // Assert - Token should be refreshed
            Assert.Equal(2, tokenRequestCount);
        }

        [Fact]
        public async Task RefreshToken_WithRetry_ShouldRetryOnFailure()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int attemptCount = 0;

            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    attemptCount++;
                    if (attemptCount < 3)
                    {
                        // Fail first 2 attempts
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.ServiceUnavailable,
                            Content = new StringContent("Service Unavailable")
                        };
                    }
                    // Succeed on 3rd attempt
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"access_token\":\"test_token\",\"expires_in\":3600}")
                    };
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var options = new OAuthOptions
            {
                MaxRetryAttempts = 3,
                InitialBackoffDelay = TimeSpan.FromMilliseconds(10)
            };

            var handler = new OAuth2ClientCredentialsHandler(
                "https://auth.example.com/token",
                "client_id",
                "client_secret",
                options: options,
                httpClient: httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            await handler.ApplyAsync(request);

            // Assert
            Assert.Equal(3, attemptCount);
            Assert.Equal("test_token", request.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task RefreshToken_ExceedMaxRetries_ShouldThrowException()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();

            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    Content = new StringContent("Service Unavailable")
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var options = new OAuthOptions
            {
                MaxRetryAttempts = 2,
                InitialBackoffDelay = TimeSpan.FromMilliseconds(10)
            };

            var handler = new OAuth2ClientCredentialsHandler(
                "https://auth.example.com/token",
                "client_id",
                "client_secret",
                options: options,
                httpClient: httpClient);

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(
                async () => await handler.ApplyAsync(request));
        }

        [Fact]
        public async Task ConcurrentRequests_ShouldOnlyRefreshOnce()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int tokenRequestCount = 0;

            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(async () =>
                {
                    tokenRequestCount++;
                    await Task.Delay(100); // Simulate network delay
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"access_token\":\"test_token\",\"expires_in\":3600}")
                    };
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var handler = new OAuth2ClientCredentialsHandler(
                "https://auth.example.com/token",
                "client_id",
                "client_secret",
                httpClient: httpClient);

            // Act - Send 5 concurrent requests
            var tasks = new Task[5];
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = Task.Run(async () =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
                    await handler.ApplyAsync(request);
                });
            }

            await Task.WhenAll(tasks);

            // Assert - Token should only be requested once despite concurrent requests
            Assert.Equal(1, tokenRequestCount);
        }

        [Fact]
        public async Task ForceRefreshAsync_ShouldRequestNewToken()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            int tokenRequestCount = 0;

            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri.ToString().Contains("/token")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    tokenRequestCount++;
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent($"{{\"access_token\":\"token_{tokenRequestCount}\",\"expires_in\":3600}}")
                    };
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var handler = new OAuth2ClientCredentialsHandler(
                "https://auth.example.com/token",
                "client_id",
                "client_secret",
                httpClient: httpClient);

            // First apply
            var request1 = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test1");
            await handler.ApplyAsync(request1);
            Assert.Equal("token_1", request1.Headers.Authorization.Parameter);

            // Force refresh
            await handler.ForceRefreshAsync();
            Assert.Equal(2, tokenRequestCount);

            // Next apply should use new token
            var request2 = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test2");
            await handler.ApplyAsync(request2);
            Assert.Equal("token_2", request2.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task OnAuthenticationFailure_ShouldFireEvent()
        {
            // Arrange
            var mockHttpHandler = new Mock<HttpMessageHandler>();
            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable
                });

            var httpClient = new HttpClient(mockHttpHandler.Object);
            var options = new OAuthOptions
            {
                MaxRetryAttempts = 1,
                InitialBackoffDelay = TimeSpan.FromMilliseconds(1)
            };

            var handler = new OAuth2ClientCredentialsHandler(
                "https://auth.example.com/token",
                "client_id",
                "client_secret",
                options: options,
                httpClient: httpClient);

            bool eventFired = false;
            handler.OnAuthenticationFailure += (sender, args) =>
            {
                eventFired = true;
                Assert.NotNull(args.Exception);
                Assert.Equal(1, args.RetryCount);
            };

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(async () => await handler.ApplyAsync(request));
            Assert.True(eventFired);
        }
    }
}
