using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestFlow.Client20.Models;

namespace RestFlow.Client20.Handlers.Authentication
{
    /// <summary>
    /// OAuth 2.0 Client Credentials grant authentication handler
    /// </summary>
    public class OAuth2ClientCredentialsHandler : IOAuthHandler
    {
        private readonly string _tokenEndpoint;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _scope;
        private readonly OAuthOptions _options;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        private string _token;
        private DateTime _expiresAt;

        /// <summary>
        /// Event raised when authentication fails
        /// </summary>
        public event EventHandler<AuthFailureEventArgs> OnAuthenticationFailure;

        public OAuth2ClientCredentialsHandler(
            string tokenEndpoint,
            string clientId,
            string clientSecret,
            string scope = null,
            OAuthOptions options = null,
            HttpClient httpClient = null)
        {
            _tokenEndpoint = tokenEndpoint ?? throw new ArgumentNullException(nameof(tokenEndpoint));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _scope = scope;
            _options = options ?? new OAuthOptions();
            _httpClient = httpClient ?? new HttpClient();
            _expiresAt = DateTime.MinValue;
        }

        public async Task ApplyAsync(HttpRequestMessage request)
        {
            // Ensure we have a valid token
            if (IsTokenExpired())
            {
                await RefreshTokenAsync();
            }

            // Apply the token to the request
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        /// <summary>
        /// Forces a token refresh regardless of expiration status
        /// </summary>
        public async Task ForceRefreshAsync()
        {
            // Force expiration
            _expiresAt = DateTime.MinValue;
            await RefreshTokenAsync();
        }

        /// <summary>
        /// Gets the OAuth options for this handler
        /// </summary>
        public OAuthOptions GetOptions()
        {
            return _options;
        }

        private bool IsTokenExpired()
        {
            if (string.IsNullOrEmpty(_token))
            {
                return true;
            }

            // Handle edge case where _expiresAt is too small to subtract clock skew
            if (_expiresAt == DateTime.MinValue)
            {
                return true;
            }

            // Apply clock skew - refresh before actual expiration
            var expirationWithSkew = _expiresAt.AddSeconds(-_options.ClockSkewSeconds);
            return _options.TimeProvider.UtcNow >= expirationWithSkew;
        }

        private async Task RefreshTokenAsync()
        {
            // Wait for the lock
            await _refreshLock.WaitAsync();
            
            try
            {
                // Double-check pattern - another thread may have already refreshed
                if (!IsTokenExpired())
                {
                    return;
                }

                int retryCount = 0;
                TimeSpan delay = _options.InitialBackoffDelay;

                while (retryCount < _options.MaxRetryAttempts)
                {
                    try
                    {
                        var tokenResponse = await RequestTokenAsync();
                        _token = tokenResponse.AccessToken;
                        _expiresAt = _options.TimeProvider.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                        return; // Success
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        
                        if (retryCount >= _options.MaxRetryAttempts)
                        {
                            OnAuthenticationFailure?.Invoke(this, new AuthFailureEventArgs(ex, retryCount));
                            throw new ApiException(
                                $"Failed to refresh OAuth token after {retryCount} attempts", 
                                HttpStatusCode.Unauthorized, 
                                ex);
                        }

                        // Exponential backoff
                        await Task.Delay(delay);
                        delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                    }
                }
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private async Task<TokenResponse> RequestTokenAsync()
        {
            var requestBody = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _clientId },
                { "client_secret", _clientSecret }
            };

            if (!string.IsNullOrEmpty(_scope))
            {
                requestBody["scope"] = _scope;
            }

            var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(requestBody)
            };

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ApiException(
                    $"Token request failed: {response.StatusCode}", 
                    response.StatusCode,
                    responseBody: errorContent);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            return ParseTokenResponse(responseContent);
        }

        private TokenResponse ParseTokenResponse(string json)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    // Case-insensitive parsing
                    MetadataPropertyHandling = MetadataPropertyHandling.Ignore
                };

                var jObject = JObject.Parse(json);
                
                // Support both snake_case and camelCase
                var accessToken = GetTokenValue(jObject, "access_token", "accessToken");
                var expiresIn = GetIntValue(jObject, "expires_in", "expiresIn");

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("access_token not found in token response");
                }

                return new TokenResponse
                {
                    AccessToken = accessToken,
                    ExpiresIn = expiresIn > 0 ? expiresIn : 3600 // Default to 1 hour if not specified
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse token response: {ex.Message}", ex);
            }
        }

        private string GetTokenValue(JObject jObject, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                var token = jObject[name];
                if (token != null)
                {
                    return token.Value<string>();
                }
            }
            return null;
        }

        private int GetIntValue(JObject jObject, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                var token = jObject[name];
                if (token != null)
                {
                    return token.Value<int>();
                }
            }
            return 0;
        }

        internal string GetCurrentToken() => _token;
        internal DateTime GetTokenExpiration() => _expiresAt;

        private class TokenResponse
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
        }
    }

    /// <summary>
    /// Event arguments for authentication failures
    /// </summary>
    public class AuthFailureEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public int RetryCount { get; }

        public AuthFailureEventArgs(Exception exception, int retryCount)
        {
            Exception = exception;
            RetryCount = retryCount;
        }
    }
}
