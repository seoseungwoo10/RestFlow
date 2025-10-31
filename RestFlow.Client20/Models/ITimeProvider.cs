using System;

namespace RestFlow.Client20.Models
{
    /// <summary>
    /// Time provider interface for testability
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the current UTC time
        /// </summary>
        DateTime UtcNow { get; }
    }

    /// <summary>
    /// System time provider implementation
    /// </summary>
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
