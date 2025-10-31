using System.Net.Http;
using System.Threading.Tasks;

namespace RestFlow.Client20.Handlers.Authentication
{
    /// <summary>
    /// No authentication handler - allows requests without authentication
    /// </summary>
    public class NoAuthHandler : IAuthenticationHandler
    {
        public Task ApplyAsync(HttpRequestMessage request)
        {
            // No authentication to apply
            return Task.CompletedTask;
        }
    }
}
