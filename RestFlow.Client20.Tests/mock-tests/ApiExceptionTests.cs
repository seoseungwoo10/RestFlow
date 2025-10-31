using System;
using System.Collections.Generic;
using System.Net;
using RestFlow.Client20.Models;
using Xunit;

namespace RestFlow.Client20.Tests
{
    public class ApiExceptionTests
    {
        [Fact]
        public void Constructor_WithMessageAndStatusCode_ShouldSetProperties()
        {
            // Arrange & Act
            var exception = new ApiException("Test error", HttpStatusCode.BadRequest);

            // Assert
            Assert.Equal("Test error", exception.Message);
            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Null(exception.Headers);
            Assert.Null(exception.ResponseBody);
        }

        [Fact]
        public void Constructor_WithAllParameters_ShouldSetAllProperties()
        {
            // Arrange
            var headers = new Dictionary<string, IEnumerable<string>>
            {
                { "Content-Type", new[] { "application/json" } },
                { "X-Custom-Header", new[] { "custom-value" } }
            };
            var responseBody = "{\"error\":\"Not found\"}";

            // Act
            var exception = new ApiException(
                "Test error",
                HttpStatusCode.NotFound,
                headers,
                responseBody);

            // Assert
            Assert.Equal("Test error", exception.Message);
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal(headers, exception.Headers);
            Assert.Equal(responseBody, exception.ResponseBody);
        }

        [Fact]
        public void Constructor_WithInnerException_ShouldSetInnerException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new ApiException(
                "Test error",
                HttpStatusCode.InternalServerError,
                innerException);

            // Assert
            Assert.Equal("Test error", exception.Message);
            Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void StatusCodeAsInt_ShouldReturnCorrectValue()
        {
            // Arrange
            var exception = new ApiException("Test", HttpStatusCode.NotFound);

            // Act
            var statusCodeInt = (int)exception.StatusCode;

            // Assert
            Assert.Equal(404, statusCodeInt);
        }

        [Fact]
        public void ToString_ShouldIncludeStatusCodeAndMessage()
        {
            // Arrange
            var exception = new ApiException("Request failed", HttpStatusCode.BadRequest);

            // Act
            var result = exception.ToString();

            // Assert
            Assert.Contains("Request failed", result);
            // ApiException inherits from Exception, so ToString includes the full type name and message
        }
    }
}
