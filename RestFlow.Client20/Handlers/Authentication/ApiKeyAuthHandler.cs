using System;
using System.Net.Http;
using System.Threading.Tasks;
using RestFlow.Client20.Models;

namespace RestFlow.Client20.Handlers.Authentication
{
    /// <summary>
    /// API Key authentication handler
    /// </summary>
    public class ApiKeyAuthHandler : IAuthenticationHandler
    {
        private readonly string _key;
        private readonly string _value;
        private readonly ApiKeyLocation _location;

        public ApiKeyAuthHandler(string key, string value, ApiKeyLocation location = ApiKeyLocation.Header)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _location = location;
        }

        public Task ApplyAsync(HttpRequestMessage request)
        {
            if (_location == ApiKeyLocation.Header)
            {
                request.Headers.Add(_key, _value);
            }
            else if (_location == ApiKeyLocation.QueryParam)
            {
                var uriBuilder = new UriBuilder(request.RequestUri);
                var query = uriBuilder.Query;
                
                if (string.IsNullOrEmpty(query))
                {
                    uriBuilder.Query = $"{_key}={Uri.EscapeDataString(_value)}";
                }
                else
                {
                    uriBuilder.Query = query.Substring(1) + $"&{_key}={Uri.EscapeDataString(_value)}";
                }
                
                request.RequestUri = uriBuilder.Uri;
            }

            return Task.CompletedTask;
        }
    }
}
