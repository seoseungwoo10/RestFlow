using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RestFlow.Client20.Handlers.Authentication
{
    /// <summary>
    /// Bearer token authentication handler
    /// </summary>
    public class BearerTokenAuthHandler : IAuthenticationHandler
    {
        private readonly string _token;

        public BearerTokenAuthHandler(string token)
        {
            _token = token ?? throw new ArgumentNullException(nameof(token));
        }

        public Task ApplyAsync(HttpRequestMessage request)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            return Task.CompletedTask;
        }
    }
}
