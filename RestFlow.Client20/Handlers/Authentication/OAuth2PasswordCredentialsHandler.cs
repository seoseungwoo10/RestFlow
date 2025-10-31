using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestFlow.Client20.Models;

namespace RestFlow.Client20.Handlers.Authentication
{
    /// <summary>
    /// OAuth 2.0 Password Credentials grant authentication handler
    /// </summary>
    public class OAuth2PasswordCredentialsHandler : IOAuthHandler
    {
        private readonly string _tokenEndpoint;
        private readonly string _username;
        private readonly string _password;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly OAuthOptions _options;
        private readonly HttpClient _httpClient;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);

        private string _token;
        private DateTime _expiresAt;

        public event EventHandler<AuthFailureEventArgs> OnAuthenticationFailure;

        public OAuth2PasswordCredentialsHandler(
            string tokenEndpoint,
            string username,
            string password,
            string clientId,
            string clientSecret = null,
            OAuthOptions options = null,
            HttpClient httpClient = null)
        {
            _tokenEndpoint = tokenEndpoint ?? throw new ArgumentNullException(nameof(tokenEndpoint));
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret;
            _options = options ?? new OAuthOptions();
            _httpClient = httpClient ?? new HttpClient();
            _expiresAt = DateTime.MinValue;
        }

        public async Task ApplyAsync(HttpRequestMessage request)
        {
            if (IsTokenExpired())
            {
                await RefreshTokenAsync();
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        /// <summary>
        /// Forces a token refresh regardless of expiration status
        /// </summary>
        public async Task ForceRefreshAsync()
        {
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

            var expirationWithSkew = _expiresAt.AddSeconds(-_options.ClockSkewSeconds);
            return _options.TimeProvider.UtcNow >= expirationWithSkew;
        }

        private async Task RefreshTokenAsync()
        {
            await _refreshLock.WaitAsync();
            
            try
            {
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
                        return;
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
                { "grant_type", "password" },
                { "username", _username },
                { "password", _password },
                { "client_id", _clientId }
            };

            if (!string.IsNullOrEmpty(_clientSecret))
            {
                requestBody["client_secret"] = _clientSecret;
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
                var jObject = JObject.Parse(json);
                
                var accessToken = GetTokenValue(jObject, "access_token", "accessToken");
                var expiresIn = GetIntValue(jObject, "expires_in", "expiresIn");

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("access_token not found in token response");
                }

                return new TokenResponse
                {
                    AccessToken = accessToken,
                    ExpiresIn = expiresIn > 0 ? expiresIn : 3600
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
}
