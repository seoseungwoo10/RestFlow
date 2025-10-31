using System;
using System.Collections.Generic;
using System.Net;

namespace RestFlow.Client20.Models
{
    /// <summary>
    /// Exception thrown when API requests fail
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// HTTP status code of the response
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Response headers
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; }

        /// <summary>
        /// Response body content
        /// </summary>
        public string ResponseBody { get; }

        public ApiException(
            string message, 
            HttpStatusCode statusCode, 
            IReadOnlyDictionary<string, IEnumerable<string>> headers = null,
            string responseBody = null) 
            : base(message)
        {
            StatusCode = statusCode;
            Headers = headers;
            ResponseBody = responseBody;
        }

        public ApiException(
            string message, 
            HttpStatusCode statusCode, 
            Exception innerException,
            IReadOnlyDictionary<string, IEnumerable<string>> headers = null,
            string responseBody = null) 
            : base(message, innerException)
        {
            StatusCode = statusCode;
            Headers = headers;
            ResponseBody = responseBody;
        }
    }
}
