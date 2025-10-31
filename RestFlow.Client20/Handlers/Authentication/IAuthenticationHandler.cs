using System.Net.Http;
using System.Threading.Tasks;

namespace RestFlow.Client20.Handlers.Authentication
{
    /// <summary>
    /// Interface for authentication handlers that apply authentication to HTTP requests
    /// </summary>
    public interface IAuthenticationHandler
    {
        /// <summary>
        /// Applies authentication to the HTTP request
        /// </summary>
        /// <param name="request">The HTTP request message to authenticate</param>
        Task ApplyAsync(HttpRequestMessage request);
    }
}
