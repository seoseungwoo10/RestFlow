using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestFlow.Client20.Handlers.Authentication;
using RestFlow.Client20.Models;

namespace RestFlow.Client20
{
    /// <summary>
    /// RestFlow HTTP client for making REST API calls with various authentication methods
    /// </summary>
    public class RestFlowClient
    {
        private readonly HttpClient _httpClient;
        private IAuthenticationHandler _authHandler;
        private string _baseUrl;
        private readonly bool _disposeHttpClient;
        private const string RetryAttemptHeader = "X-RestFlow-Retry-Attempt";

        /// <summary>
        /// Initializes a new instance of RestFlowClient
        /// </summary>
        public RestFlowClient() : this(new HttpClient(), true)
        {
        }

        /// <summary>
        /// Initializes a new instance of RestFlowClient with a custom HttpClient
        /// </summary>
        /// <param name="httpClient">Custom HttpClient instance</param>
        /// <param name="disposeHttpClient">Whether to dispose the HttpClient when this instance is disposed</param>
        public RestFlowClient(HttpClient httpClient, bool disposeHttpClient = false)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _disposeHttpClient = disposeHttpClient;
            _authHandler = new NoAuthHandler();
        }

        /// <summary>
        /// Sets the base URL for all requests
        /// </summary>
        public RestFlowClient WithBaseUrl(string url)
        {
            _baseUrl = url?.TrimEnd('/');
            return this;
        }

        /// <summary>
        /// Sets a custom authentication handler
        /// </summary>
        public RestFlowClient WithAuthentication(IAuthenticationHandler handler)
        {
            _authHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            return this;
        }

        /// <summary>
        /// Configures Basic authentication
        /// </summary>
        public RestFlowClient WithBasicAuth(string username, string password)
        {
            _authHandler = new BasicAuthHandler(username, password);
            return this;
        }

        /// <summary>
        /// Configures Bearer token authentication
        /// </summary>
        public RestFlowClient WithBearerToken(string token)
        {
            _authHandler = new BearerTokenAuthHandler(token);
            return this;
        }

        /// <summary>
        /// Configures API key authentication
        /// </summary>
        public RestFlowClient WithApiKey(string key, string value, ApiKeyLocation location = ApiKeyLocation.Header)
        {
            _authHandler = new ApiKeyAuthHandler(key, value, location);
            return this;
        }

        /// <summary>
        /// Configures OAuth 2.0 Client Credentials grant
        /// </summary>
        public RestFlowClient WithOAuthClientCredentials(
            string tokenEndpoint, 
            string clientId, 
            string clientSecret, 
            string scope = null, 
            OAuthOptions options = null)
        {
            _authHandler = new OAuth2ClientCredentialsHandler(
                tokenEndpoint, 
                clientId, 
                clientSecret, 
                scope, 
                options,
                _httpClient); // Pass the HttpClient to the handler
            return this;
        }

        /// <summary>
        /// Configures OAuth 2.0 Password Credentials grant
        /// </summary>
        public RestFlowClient WithOAuthPasswordCredentials(
            string tokenEndpoint,
            string username,
            string password,
            string clientId,
            string clientSecret = null,
            OAuthOptions options = null)
        {
            _authHandler = new OAuth2PasswordCredentialsHandler(
                tokenEndpoint,
                username,
                password,
                clientId,
                clientSecret,
                options,
                _httpClient); // Pass the HttpClient to the handler
            return this;
        }

        /// <summary>
        /// Configures OAuth 2.0 Authorization Code grant (refresh token flow)
        /// </summary>
        public RestFlowClient WithOAuthAuthorizationCode(
            string tokenEndpoint,
            string refreshToken,
            string clientId,
            string clientSecret,
            OAuthOptions options = null)
        {
            _authHandler = new OAuth2AuthorizationCodeHandler(
                tokenEndpoint,
                refreshToken,
                clientId,
                clientSecret,
                options,
                _httpClient); // Pass the HttpClient to the handler
            return this;
        }

        /// <summary>
        /// Sends a GET request and deserializes the response
        /// </summary>
        public async Task<T> GetAsync<T>(string url)
        {
            var request = CreateRequest(HttpMethod.Get, url);
            return await SendAsync<T>(request);
        }

        /// <summary>
        /// Sends a POST request with a body and deserializes the response
        /// </summary>
        public async Task<T> PostAsync<T>(string url, object body)
        {
            var request = CreateRequest(HttpMethod.Post, url);
            SetJsonContent(request, body);
            return await SendAsync<T>(request);
        }

        /// <summary>
        /// Sends a POST request with a body
        /// </summary>
        public async Task PostAsync(string url, object body)
        {
            var request = CreateRequest(HttpMethod.Post, url);
            SetJsonContent(request, body);
            await SendAsync(request);
        }

        /// <summary>
        /// Sends a PUT request with a body and deserializes the response
        /// </summary>
        public async Task<T> PutAsync<T>(string url, object body)
        {
            var request = CreateRequest(HttpMethod.Put, url);
            SetJsonContent(request, body);
            return await SendAsync<T>(request);
        }

        /// <summary>
        /// Sends a PUT request with a body
        /// </summary>
        public async Task PutAsync(string url, object body)
        {
            var request = CreateRequest(HttpMethod.Put, url);
            SetJsonContent(request, body);
            await SendAsync(request);
        }

        /// <summary>
        /// Sends a DELETE request
        /// </summary>
        public async Task DeleteAsync(string url)
        {
            var request = CreateRequest(HttpMethod.Delete, url);
            await SendAsync(request);
        }

        /// <summary>
        /// Sends a PATCH request with a body and deserializes the response
        /// </summary>
        public async Task<T> PatchAsync<T>(string url, object body)
        {
            var request = CreateRequest(new HttpMethod("PATCH"), url);
            SetJsonContent(request, body);
            return await SendAsync<T>(request);
        }

        /// <summary>
        /// Sends a PATCH request with a body
        /// </summary>
        public async Task PatchAsync(string url, object body)
        {
            var request = CreateRequest(new HttpMethod("PATCH"), url);
            SetJsonContent(request, body);
            await SendAsync(request);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var fullUrl = string.IsNullOrEmpty(_baseUrl) 
                ? url 
                : $"{_baseUrl}/{url.TrimStart('/')}?";

            return new HttpRequestMessage(method, fullUrl);
        }

        private void SetJsonContent(HttpRequestMessage request, object body)
        {
            if (body != null)
            {
                var json = JsonConvert.SerializeObject(body);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }
        }

        private async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            var response = await SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            
            if (string.IsNullOrWhiteSpace(content))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(content);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            // Apply authentication
            await _authHandler.ApplyAsync(request);

            // Send request
            var response = await _httpClient.SendAsync(request);

            // Handle 401 Unauthorized with automatic retry
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Check if retry is enabled and if we haven't already retried
                var retryAttempt = GetRetryAttempt(request);
                
                if (retryAttempt == 0 && _authHandler is IOAuthHandler oauthHandler)
                {
                    var options = oauthHandler.GetOptions();
                    if (options?.EnableAutoRetryOn401 == true)
                    {
                        response.Dispose();
                        
                        // Force token refresh
                        await oauthHandler.ForceRefreshAsync();
                        
                        // Clone the request for retry
                        var retryRequest = await CloneHttpRequestAsync(request);
                        retryRequest.Headers.Add(RetryAttemptHeader, "1");
                        
                        // Apply refreshed authentication
                        await _authHandler.ApplyAsync(retryRequest);
                        
                        // Retry the request (direct call to avoid recursion)
                        response = await _httpClient.SendAsync(retryRequest);
                        
                        // If still 401 after retry, throw exception
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var headers = response.Headers.ToDictionary(
                                h => h.Key,
                                h => (IEnumerable<string>)h.Value);

                            throw new ApiException(
                                $"Request failed with status {response.StatusCode} after token refresh retry",
                                response.StatusCode,
                                headers,
                                content);
                        }
                    }
                }
            }

            // Handle other errors
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var headers = response.Headers.ToDictionary(
                    h => h.Key,
                    h => (IEnumerable<string>)h.Value);

                throw new ApiException(
                    $"Request failed with status {response.StatusCode}",
                    response.StatusCode,
                    headers,
                    content);
            }

            return response;
        }

        private int GetRetryAttempt(HttpRequestMessage request)
        {
            if (request.Headers.TryGetValues(RetryAttemptHeader, out var values))
            {
                var value = values.FirstOrDefault();
                if (int.TryParse(value, out int attempt))
                {
                    return attempt;
                }
            }
            return 0;
        }

        private async Task<HttpRequestMessage> CloneHttpRequestAsync(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            // Copy headers
            foreach (var header in request.Headers)
            {
                if (header.Key != RetryAttemptHeader && header.Key != "Authorization")
                {
                    clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Copy content
            if (request.Content != null)
            {
                var contentBytes = await request.Content.ReadAsByteArrayAsync();
                clone.Content = new ByteArrayContent(contentBytes);

                // Copy content headers
                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }

        /// <summary>
        /// Disposes the RestFlowClient and optionally the HttpClient
        /// </summary>
        public void Dispose()
        {
            if (_disposeHttpClient)
            {
                _httpClient?.Dispose();
            }
        }
    }
}
