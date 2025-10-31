using System;

namespace RestFlow.Client20.Models
{
    /// <summary>
    /// Configuration options for OAuth 2.0 authentication
    /// </summary>
    public class OAuthOptions
    {
        /// <summary>
        /// Time provider for clock management (default: SystemTimeProvider)
        /// </summary>
        public ITimeProvider TimeProvider { get; set; } = new SystemTimeProvider();

        /// <summary>
        /// Clock skew compensation time in seconds (default: 120 seconds)
        /// Tokens will be refreshed this many seconds before actual expiration
        /// </summary>
        public int ClockSkewSeconds { get; set; } = 120;

        /// <summary>
        /// Maximum retry attempts for token refresh failures (default: 3)
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Initial backoff delay for retry (default: 1 second)
        /// </summary>
        public TimeSpan InitialBackoffDelay { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Enable automatic retry on 401 Unauthorized responses (default: true)
        /// </summary>
        public bool EnableAutoRetryOn401 { get; set; } = true;
    }
}
