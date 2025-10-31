namespace RestFlow.Client20.Models
{
    /// <summary>
    /// Location where API key should be placed
    /// </summary>
    public enum ApiKeyLocation
    {
        /// <summary>
        /// API key in HTTP header
        /// </summary>
        Header,

        /// <summary>
        /// API key in query parameter
        /// </summary>
        QueryParam
    }
}
