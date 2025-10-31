using System;
using System.Net.Http;
using System.Threading.Tasks;
using RestFlow.Client20.Handlers.Authentication;
using RestFlow.Client20.Models;
using Xunit;

namespace RestFlow.Client20.Tests
{
    public class AuthenticationHandlerTests
    {
        [Fact]
        public async Task BasicAuthHandler_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var handler = new BasicAuthHandler("testuser", "testpass");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            await handler.ApplyAsync(request);

            // Assert
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Basic", request.Headers.Authorization.Scheme);
            
            // Decode and verify
            var credentials = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(request.Headers.Authorization.Parameter));
            Assert.Equal("testuser:testpass", credentials);
        }

        [Fact]
        public async Task BearerTokenAuthHandler_ShouldSetAuthorizationHeader()
        {
            // Arrange
            var handler = new BearerTokenAuthHandler("my_bearer_token");
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            await handler.ApplyAsync(request);

            // Assert
            Assert.NotNull(request.Headers.Authorization);
            Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
            Assert.Equal("my_bearer_token", request.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task ApiKeyAuthHandler_InHeader_ShouldSetCustomHeader()
        {
            // Arrange
            var handler = new ApiKeyAuthHandler("X-API-Key", "my_api_key", ApiKeyLocation.Header);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            await handler.ApplyAsync(request);

            // Assert
            Assert.True(request.Headers.Contains("X-API-Key"));
            var values = request.Headers.GetValues("X-API-Key");
            Assert.Contains("my_api_key", values);
        }

        [Fact]
        public async Task ApiKeyAuthHandler_InQueryString_ShouldAppendToUri()
        {
            // Arrange
            var handler = new ApiKeyAuthHandler("api_key", "my_api_key", ApiKeyLocation.QueryParam);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            await handler.ApplyAsync(request);

            // Assert
            Assert.Contains("api_key=my_api_key", request.RequestUri.Query);
        }

        [Fact]
        public async Task ApiKeyAuthHandler_InQueryString_WithExistingQuery_ShouldAppendCorrectly()
        {
            // Arrange
            var handler = new ApiKeyAuthHandler("api_key", "my_api_key", ApiKeyLocation.QueryParam);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test?existing=value");

            // Act
            await handler.ApplyAsync(request);

            // Assert
            var query = request.RequestUri.Query;
            Assert.Contains("existing=value", query);
            Assert.Contains("api_key=my_api_key", query);
        }

        [Fact]
        public async Task NoAuthHandler_ShouldNotModifyRequest()
        {
            // Arrange
            var handler = new NoAuthHandler();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            var originalUri = request.RequestUri;

            // Act
            await handler.ApplyAsync(request);

            // Assert
            Assert.Null(request.Headers.Authorization);
            Assert.Equal(originalUri, request.RequestUri);
        }

        [Fact]
        public void BasicAuthHandler_WithNullUsername_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BasicAuthHandler(null, "password"));
        }

        [Fact]
        public void BasicAuthHandler_WithNullPassword_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BasicAuthHandler("username", null));
        }

        [Fact]
        public void BearerTokenAuthHandler_WithNullToken_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BearerTokenAuthHandler(null));
        }

        [Fact]
        public void ApiKeyAuthHandler_WithNullKey_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApiKeyAuthHandler(null, "value"));
        }

        [Fact]
        public void ApiKeyAuthHandler_WithNullValue_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ApiKeyAuthHandler("key", null));
        }
    }
}
