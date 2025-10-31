using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RestFlow.Client20.Handlers.Authentication
{
    /// <summary>
    /// Basic authentication handler
    /// </summary>
    public class BasicAuthHandler : IAuthenticationHandler
    {
        private readonly string _username;
        private readonly string _password;

        public BasicAuthHandler(string username, string password)
        {
            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public Task ApplyAsync(HttpRequestMessage request)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            return Task.CompletedTask;
        }
    }
}
