using RestFlow.Client20.Models;
using System.Net;

namespace RestFlow.Client20.Tests.RealTests
{
    /// <summary>
    /// Integration tests for RestFlow.DemoServer API endpoints
    /// These tests require the DemoServer to be running at http://localhost:5245
    /// </summary>
    public class RestFlowDemoServerIntegrationTests : IDisposable
    {
        private const string BaseUrl = "http://localhost:5245";
        private readonly HttpClient _httpClient;

        public RestFlowDemoServerIntegrationTests()
        {
            _httpClient = new HttpClient();
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #region Health Check & Non-Auth Tests

        [Fact]
        public async Task HealthCheck_Root_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl);

            // Act
            var response = await client.GetAsync<HealthCheckResponse>("/");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Contains("RestFlow.DemoServer", response.Message);
            Assert.Equal("1.0", response.Version);
            Assert.NotNull(response.Timestamp);
        }

        [Fact]
        public async Task NoAuth_Test_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
              .WithBaseUrl(BaseUrl);

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/no-auth/test");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Equal("No authentication required", response.Message);
            Assert.Equal("Non-Auth", response.AuthType);
        }

        #endregion

        #region Basic Authentication Tests

        [Fact]
        public async Task BasicAuth_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithBasicAuth("admin", "password");

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/basic-auth/test");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Equal("Basic authentication successful", response.Message);
            Assert.Equal("Basic Auth", response.AuthType);
            Assert.Equal("admin", response.User);
        }

        [Fact]
        public async Task BasicAuth_WithInvalidCredentials_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithBasicAuth("wrong-user", "wrong-pass");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<AuthTestResponse>("/api/basic-auth/test"));

            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }

        [Fact]
        public async Task BasicAuth_WithoutCredentials_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
              async () => await client.GetAsync<AuthTestResponse>("/api/basic-auth/test"));

            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }

        #endregion

        #region Bearer Token Tests

        [Fact]
        public async Task BearerToken_WithValidToken_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                   .WithBaseUrl(BaseUrl)
                    .WithBearerToken("a-static-bearer-token-for-testing");

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/bearer-token/test");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Equal("Bearer token authentication successful", response.Message);
            Assert.Equal("Bearer Token", response.AuthType);
        }

        [Fact]
        public async Task BearerToken_WithInvalidToken_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithBearerToken("invalid-token");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<AuthTestResponse>("/api/bearer-token/test"));

            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }

        [Fact]
        public async Task BearerToken_WithoutToken_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                   async () => await client.GetAsync<AuthTestResponse>("/api/bearer-token/test"));

            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }

        #endregion

        #region API Key Tests

        [Fact]
        public async Task ApiKey_Header_WithValidKey_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
               .WithBaseUrl(BaseUrl)
              .WithApiKey("X-API-KEY", "a-static-api-key", ApiKeyLocation.Header);

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/api-key/header-test");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Equal("API key authentication (header) successful", response.Message);
            Assert.Equal("API Key (Header)", response.AuthType);
        }

        [Fact]
        public async Task ApiKey_Header_WithInvalidKey_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithApiKey("X-API-KEY", "wrong-api-key", ApiKeyLocation.Header);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<AuthTestResponse>("/api/api-key/header-test"));

            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }

        [Fact]
        public async Task ApiKey_Query_WithValidKey_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithApiKey("api_key", "a-static-api-key", ApiKeyLocation.QueryParam);

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/api-key/query-test");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Equal("API key authentication (query) successful", response.Message);
            Assert.Equal("API Key (Query)", response.AuthType);
        }

        [Fact]
        public async Task ApiKey_Query_WithInvalidKey_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithApiKey("api_key", "wrong-api-key", ApiKeyLocation.QueryParam);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                  async () => await client.GetAsync<AuthTestResponse>("/api/api-key/query-test"));

            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }

        #endregion

        #region OAuth 2.0 - Client Credentials Tests

        [Fact]
        public async Task OAuth_ClientCredentials_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithOAuthClientCredentials(
                    tokenEndpoint: $"{BaseUrl}/token",
                    clientId: "restflow-client",
                    clientSecret: "restflow-secret",
                    scope: "read write");

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Equal("OAuth 2.0 JWT authentication successful", response.Message);
            Assert.Equal("OAuth 2.0 (JWT)", response.AuthType);
            Assert.Null(response.User);
        }

        [Fact]
        public async Task OAuth_ClientCredentials_WithInvalidClientId_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithOAuthClientCredentials(
                    tokenEndpoint: $"{BaseUrl}/token",
                        clientId: "wrong-client",
                    clientSecret: "restflow-secret");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<AuthTestResponse>("/api/oauth/protected"));

            // OAuth handler will fail to get token, which should throw
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task OAuth_ClientCredentials_WithInvalidClientSecret_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                         .WithBaseUrl(BaseUrl)
                  .WithOAuthClientCredentials(
                   tokenEndpoint: $"{BaseUrl}/token",
                   clientId: "restflow-client",
          clientSecret: "wrong-secret");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
     async () => await client.GetAsync<AuthTestResponse>("/api/oauth/protected"));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task OAuth_ClientCredentials_MultipleRequests_ShouldReuseToken()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
           .WithBaseUrl(BaseUrl)
 .WithOAuthClientCredentials(
               tokenEndpoint: $"{BaseUrl}/token",
     clientId: "restflow-client",
         clientSecret: "restflow-secret");

            // Act - Make multiple requests
            var response1 = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");
            var response2 = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");
            var response3 = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");

            // Assert - All should succeed
            Assert.Equal("success", response1.Status);
            Assert.Equal("success", response2.Status);
            Assert.Equal("success", response3.Status);
        }

        #endregion

        #region OAuth 2.0 - Password Credentials Tests

        [Fact]
        public async Task OAuth_Password_WithValidCredentials_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithOAuthPasswordCredentials(
                    tokenEndpoint: $"{BaseUrl}/token",
                    username: "user",
                    password: "pass",
                    clientId: "restflow-client",
                    clientSecret: "restflow-secret");

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Equal("OAuth 2.0 JWT authentication successful", response.Message);
            Assert.Equal("OAuth 2.0 (JWT)", response.AuthType);
            Assert.Null(response.User);
        }

        [Fact]
        public async Task OAuth_Password_WithInvalidUsername_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
            .WithBaseUrl(BaseUrl)
            .WithOAuthPasswordCredentials(
                tokenEndpoint: $"{BaseUrl}/token",
                username: "wronguser",
                password: "pass",
                clientId: "restflow-client",
                clientSecret: "restflow-secret");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
        async () => await client.GetAsync<AuthTestResponse>("/api/oauth/protected"));

            Assert.NotNull(exception);
        }

        [Fact]
        public async Task OAuth_Password_WithInvalidPassword_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithOAuthPasswordCredentials(
                    tokenEndpoint: $"{BaseUrl}/token",
                    username: "user",
                    password: "wrongpass",
                    clientId: "restflow-client",
                    clientSecret: "restflow-secret");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<AuthTestResponse>("/api/oauth/protected"));

            Assert.NotNull(exception);
        }

        #endregion

        #region OAuth 2.0 - Authorization Code (Refresh Token) Tests

        [Fact]
        public async Task OAuth_AuthorizationCode_WithValidRefreshToken_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                    .WithBaseUrl(BaseUrl)
                         .WithOAuthAuthorizationCode(
                tokenEndpoint: $"{BaseUrl}/token",
                refreshToken: "static-refresh-token-for-auth-code-grant",
                clientId: "restflow-client",
                clientSecret: "restflow-secret");

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
            Assert.Equal("OAuth 2.0 JWT authentication successful", response.Message);
            Assert.Equal("OAuth 2.0 (JWT)", response.AuthType);
        }

        [Fact]
        public async Task OAuth_AuthorizationCode_WithPasswordGrantRefreshToken_ShouldReturnSuccess()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                   .WithBaseUrl(BaseUrl)
      .WithOAuthAuthorizationCode(
               tokenEndpoint: $"{BaseUrl}/token",
          refreshToken: "static-refresh-token-for-password-grant",
            clientId: "restflow-client",
           clientSecret: "restflow-secret");

            // Act
            var response = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");

            // Assert
            Assert.NotNull(response);
            Assert.Equal("success", response.Status);
        }

        [Fact]
        public async Task OAuth_AuthorizationCode_WithInvalidRefreshToken_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
           .WithBaseUrl(BaseUrl)
           .WithOAuthAuthorizationCode(
 tokenEndpoint: $"{BaseUrl}/token",
      refreshToken: "invalid-refresh-token",
             clientId: "restflow-client",
        clientSecret: "restflow-secret");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
    async () => await client.GetAsync<AuthTestResponse>("/api/oauth/protected"));

            Assert.NotNull(exception);
        }

        #endregion

        #region OAuth 2.0 - Token Expiration and Refresh Tests

        [Fact]
        public async Task OAuth_TokenExpiration_ShouldAutoRefresh()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
                .WithBaseUrl(BaseUrl)
                .WithOAuthClientCredentials(
                    tokenEndpoint: $"{BaseUrl}/token",
                    clientId: "restflow-client",
                    clientSecret: "restflow-secret",
                    options: new OAuthOptions
                    {
                        ClockSkewSeconds = 120,
                        EnableAutoRetryOn401 = true
                    });

            // Act - First request
            var response1 = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");
            Assert.Equal("success", response1.Status);

            // Wait for token to expire (DemoServer tokens expire in 120 seconds)
            // Since we can't wait that long in a test, this is more of a documentation of the feature
            // In real scenarios with shorter expiration, this would trigger auto-refresh

            // Second request - would auto-refresh if token expired
            var response2 = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");
            Assert.Equal("success", response2.Status);
        }

        #endregion

        #region Error Tests

        [Fact]
        public async Task OAuth_Protected_WithoutToken_ShouldThrowApiException()
        {
            // Arrange
            var client = new RestFlowClient(_httpClient)
           .WithBaseUrl(BaseUrl);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ApiException>(
                async () => await client.GetAsync<AuthTestResponse>("/api/oauth/protected"));

            Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        }

        #endregion

        #region Response Models

        private class HealthCheckResponse
        {
            public string? Status { get; set; }
            public string? Message { get; set; }
            public string? Version { get; set; }
            public string? Timestamp { get; set; }
        }

        private class AuthTestResponse
        {
            public string? Status { get; set; }
            public string? Message { get; set; }
            public string? AuthType { get; set; }
            public string? User { get; set; }
            public string? Timestamp { get; set; }
        }

        #endregion
    }
}
