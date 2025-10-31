using System.Threading.Tasks;
using RestFlow.Client20.Models;

namespace RestFlow.Client20.Handlers.Authentication
{
    /// <summary>
    /// Interface for OAuth authentication handlers that support token refresh
    /// </summary>
    public interface IOAuthHandler : IAuthenticationHandler
    {
        /// <summary>
        /// Forces a token refresh regardless of expiration status
        /// </summary>
        Task ForceRefreshAsync();

        /// <summary>
        /// Gets the OAuth options for this handler
        /// </summary>
        OAuthOptions GetOptions();
    }
}
