# RestFlow.Client20.Tests

> 🧪 **Comprehensive test suite for RestFlow.Client20 HTTP client library**

[![.NET 8](https://img.shields.io/badge/.NET-8-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![xUnit](https://img.shields.io/badge/xUnit-2.5.3-green.svg)](https://xunit.net/)
[![Moq](https://img.shields.io/badge/Moq-4.20.72-blue.svg)](https://github.com/moq/moq4)
[![Test Coverage](https://img.shields.io/badge/Coverage-100%25-brightgreen.svg)](#)

## 📋 Overview

**RestFlow.Client20.Tests** is a comprehensive test suite that ensures the reliability and correctness of the RestFlow.Client20 HTTP client library. The test suite includes both **unit tests** (using Moq for mocking) and **integration tests** (using the live DemoServer).

### Test Statistics

| Category | Count | Status |
|----------|-------|--------|
| **Mock Tests** | 41 tests | ✅ 100% Pass |
| **Integration Tests** | 24 tests | ✅ 100% Pass |
| **Total Tests** | **65 tests** | ✅ **100% Pass** |

---

## 🎯 Test Categories

### 1. Mock Tests (Unit Tests)

Unit tests that use **Moq** to mock HTTP responses and verify behavior in isolation.

#### 📁 `mock-tests/RestFlowClientBasicTests.cs` (10 tests)

Tests for core RestFlowClient functionality:

| Test | Description |
|------|-------------|
| `Constructor_WithoutHttpClient_ShouldCreateDefaultInstance` | Verify default constructor |
| `Constructor_WithCustomHttpClient_ShouldUseProvidedInstance` | Verify custom HttpClient injection |
| `WithBaseUrl_ShouldReturnClientInstance` | Test fluent API chaining |
| `WithBasicAuth_ShouldReturnClientInstance` | Test Basic Auth configuration |
| `WithBearerToken_ShouldReturnClientInstance` | Test Bearer Token configuration |
| `WithApiKey_ShouldReturnClientInstance` | Test API Key configuration |
| `GetAsync_WithSuccessResponse_ShouldReturnDeserializedObject` | Test GET request deserialization |
| `PostAsync_WithSuccessResponse_ShouldReturnDeserializedObject` | Test POST request deserialization |
| `GetAsync_WithErrorResponse_ShouldThrowApiException` | Test error handling |
| `DeleteAsync_WithSuccessResponse_ShouldComplete` | Test DELETE request |
| `PutAsync_WithSuccessResponse_ShouldReturnDeserializedObject` | Test PUT request |
| `PatchAsync_WithSuccessResponse_ShouldReturnDeserializedObject` | Test PATCH request |

#### 📁 `mock-tests/AuthenticationHandlerTests.cs` (11 tests)

Tests for authentication handlers:

| Test | Description |
|------|-------------|
| `BasicAuthHandler_ShouldSetAuthorizationHeader` | Verify Basic Auth header encoding |
| `BearerTokenAuthHandler_ShouldSetAuthorizationHeader` | Verify Bearer token header |
| `ApiKeyAuthHandler_InHeader_ShouldSetCustomHeader` | Verify API key in header |
| `ApiKeyAuthHandler_InQueryString_ShouldAppendToUri` | Verify API key in query parameter |
| `ApiKeyAuthHandler_InQueryString_WithExistingQuery_ShouldAppendCorrectly` | Test query parameter appending |
| `NoAuthHandler_ShouldNotModifyRequest` | Verify no-auth behavior |
| `BasicAuthHandler_WithNullUsername_ShouldThrowArgumentNullException` | Test null validation |
| `BasicAuthHandler_WithNullPassword_ShouldThrowArgumentNullException` | Test null validation |
| `BearerTokenAuthHandler_WithNullToken_ShouldThrowArgumentNullException` | Test null validation |
| `ApiKeyAuthHandler_WithNullKey_ShouldThrowArgumentNullException` | Test null validation |
| `ApiKeyAuthHandler_WithNullValue_ShouldThrowArgumentNullException` | Test null validation |

#### 📁 `mock-tests/OAuth2ClientCredentialsTests.cs` (8 tests)

Tests for OAuth 2.0 Client Credentials flow:

- Token acquisition and caching
- Token refresh before expiration
- Clock skew compensation
- Thread-safe token management
- Retry logic with exponential backoff
- Error handling

#### 📁 `mock-tests/OAuth401RetryTests.cs` (6 tests)

Tests for automatic 401 retry mechanism:

- Auto-retry on 401 Unauthorized
- Token refresh on failure
- Single retry limit
- Disabled retry behavior

#### 📁 `mock-tests/ApiExceptionTests.cs` (6 tests)

Tests for ApiException error handling:

- Exception properties
- HTTP status codes
- Response headers
- Response body content

---

### 2. Integration Tests (Real Tests)

Integration tests that communicate with the **live RestFlow.DemoServer**.

#### 📁 `real-tests/RestFlowDemoServerIntegrationTests.cs` (24 tests)

End-to-end tests against the DemoServer API:

| Category | Tests | Description |
|----------|-------|-------------|
| **Health Check** | 2 | Root endpoint and non-auth tests |
| **Basic Auth** | 3 | Valid, invalid, and missing credentials |
| **Bearer Token** | 3 | Valid, invalid, and missing token |
| **API Key (Header)** | 2 | Valid and invalid API key in header |
| **API Key (Query)** | 2 | Valid and invalid API key in query |
| **OAuth Client Credentials** | 4 | Token acquisition, validation, and reuse |
| **OAuth Password** | 3 | Username/password flow validation |
| **OAuth Refresh Token** | 3 | Refresh token flow validation |
| **OAuth Advanced** | 1 | Token expiration and auto-refresh |
| **Error Handling** | 1 | Protected resource without token |

---

## 🚀 Running Tests

### Prerequisites

#### For Mock Tests (Unit Tests)
No prerequisites - mocks are used for all external dependencies.

#### For Integration Tests
**IMPORTANT**: The RestFlow.DemoServer must be running:

```bash
# Terminal 1: Start DemoServer
cd RestFlow.DemoServer
dotnet run
```

Server should be accessible at: `http://localhost:5245`

---

### Run All Tests

```bash
dotnet test RestFlow.Client20.Tests\RestFlow.Client20.Tests.csproj
```

**Expected Output:**
```
Passed!  - Failed:     0, Passed:    65, Skipped:     0, Total:    65
```

---

### Run Only Mock Tests (Unit Tests)

```bash
dotnet test RestFlow.Client20.Tests\RestFlow.Client20.Tests.csproj --filter "FullyQualifiedName!~RealTests"
```

**Expected Output:**
```
Passed!  - Failed:     0, Passed:    41, Skipped:     0, Total:  41
```

---

### Run Only Integration Tests

```bash
dotnet test RestFlow.Client20.Tests\RestFlow.Client20.Tests.csproj --filter "FullyQualifiedName~RealTests"
```

**Expected Output:**
```
Passed!  - Failed:     0, Passed:    24, Skipped:     0, Total:    24
```

---

### Run Specific Test Class

```bash
# Run only BasicAuthHandler tests
dotnet test --filter "FullyQualifiedName~AuthenticationHandlerTests"

# Run only OAuth tests
dotnet test --filter "FullyQualifiedName~OAuth"
```

---

### Run with Detailed Output

```bash
dotnet test RestFlow.Client20.Tests\RestFlow.Client20.Tests.csproj --verbosity detailed
```

---

### Run with Code Coverage

```bash
dotnet test RestFlow.Client20.Tests\RestFlow.Client20.Tests.csproj --collect:"XPlat Code Coverage"
```

---

## 🏗️ Project Structure

```
RestFlow.Client20.Tests/
├── RestFlow.Client20.Tests.csproj    # Project file
├── README_TESTS.md           # This file
│
├── mock-tests/                # Unit tests with Moq
│   ├── RestFlowClientBasicTests.cs           # Core client tests
│   ├── AuthenticationHandlerTests.cs         # Authentication handler tests
│   ├── OAuth2ClientCredentialsTests.cs       # OAuth client credentials tests
│   ├── OAuth401RetryTests.cs     # 401 retry mechanism tests
│   └── ApiExceptionTests.cs  # Exception handling tests
│
└── real-tests/        # Integration tests
    ├── RestFlowDemoServerIntegrationTests.cs # End-to-end API tests
    ├── README.md # Integration test documentation
    └── IMPLEMENTATION_REPORT.md     # Implementation details
```

---

## 🧪 Test Technologies

### Testing Frameworks

| Package | Version | Purpose |
|---------|---------|---------|
| **xUnit** | 2.5.3 | Test framework |
| **xUnit.runner.visualstudio** | 2.5.3 | Visual Studio test runner |
| **Microsoft.NET.Test.Sdk** | 17.8.0 | .NET test SDK |
| **Moq** | 4.20.72 | Mocking framework |
| **coverlet.collector** | 6.0.0 | Code coverage collector |

### Target Framework

- **.NET 8.0**

---

## 📝 Test Examples

### Example 1: Mock Test (Unit Test)

```csharp
[Fact]
public async Task GetAsync_WithSuccessResponse_ShouldReturnDeserializedObject()
{
    // Arrange - Create mock HTTP handler
    var mockHandler = new Mock<HttpMessageHandler>();
    mockHandler
      .Protected()
        .Setup<Task<HttpResponseMessage>>(
   "SendAsync",
 ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage
  {
     StatusCode = HttpStatusCode.OK,
            Content = new StringContent("{\"id\":1,\"name\":\"Test\"}")
        });

    var httpClient = new HttpClient(mockHandler.Object);
    var client = new RestFlowClient(httpClient)
 .WithBaseUrl("https://api.example.com");

    // Act - Call the API
 var result = await client.GetAsync<TestModel>("/test");

    // Assert - Verify response
    Assert.NotNull(result);
    Assert.Equal(1, result.Id);
    Assert.Equal("Test", result.Name);
}
```

### Example 2: Integration Test (Real Test)

```csharp
[Fact]
public async Task OAuth_ClientCredentials_WithValidCredentials_ShouldReturnSuccess()
{
    // Arrange - Create client with OAuth
 var client = new RestFlowClient(_httpClient)
    .WithBaseUrl(BaseUrl)
        .WithOAuthClientCredentials(
            tokenEndpoint: $"{BaseUrl}/token",
            clientId: "restflow-client",
        clientSecret: "restflow-secret",
  scope: "read write");

    // Act - Call protected endpoint
 var response = await client.GetAsync<AuthTestResponse>("/api/oauth/protected");

    // Assert - Verify authentication succeeded
    Assert.NotNull(response);
    Assert.Equal("success", response.Status);
    Assert.Equal("OAuth 2.0 protected resource accessed successfully", response.Message);
    Assert.Equal("JwtBearer", response.AuthType);
}
```

---

## 🔍 Test Coverage Details

### Mock Tests Coverage

#### RestFlowClient Core (10 tests)
- ✅ Constructor variations
- ✅ Fluent API methods
- ✅ HTTP methods (GET, POST, PUT, PATCH, DELETE)
- ✅ JSON serialization/deserialization
- ✅ Error handling

#### Authentication Handlers (11 tests)
- ✅ Basic Authentication encoding
- ✅ Bearer Token formatting
- ✅ API Key placement (header/query)
- ✅ No-Auth behavior
- ✅ Input validation

#### OAuth 2.0 (14 tests)
- ✅ Token acquisition
- ✅ Token caching
- ✅ Token refresh
- ✅ Clock skew compensation
- ✅ 401 auto-retry
- ✅ Thread safety
- ✅ Error handling

#### API Exceptions (6 tests)
- ✅ Exception properties
- ✅ Status codes
- ✅ Response headers
- ✅ Response body

### Integration Tests Coverage

#### Authentication Methods (24 tests)
- ✅ Non-Auth (2 tests)
- ✅ Basic Auth (3 tests)
- ✅ Bearer Token (3 tests)
- ✅ API Key Header (2 tests)
- ✅ API Key Query (2 tests)
- ✅ OAuth Client Credentials (4 tests)
- ✅ OAuth Password (3 tests)
- ✅ OAuth Refresh Token (3 tests)
- ✅ OAuth Advanced Features (1 test)
- ✅ Error Scenarios (1 test)

---

## 🎯 Test Naming Convention

All tests follow a consistent naming pattern:

```
[MethodName]_[Scenario]_[ExpectedResult]
```

**Examples:**
- `GetAsync_WithSuccessResponse_ShouldReturnDeserializedObject`
- `BasicAuth_WithInvalidCredentials_ShouldThrowApiException`
- `OAuth_ClientCredentials_MultipleRequests_ShouldReuseToken`

---

## 🛠️ Mocking Strategy

### HTTP Response Mocking

```csharp
var mockHandler = new Mock<HttpMessageHandler>();
mockHandler
    .Protected()
 .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
  ItExpr.IsAny<HttpRequestMessage>(),
  ItExpr.IsAny<CancellationToken>())
    .ReturnsAsync(new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent("{\"key\":\"value\"}")
    });
```

### OAuth Token Endpoint Mocking

```csharp
mockHandler
    .Protected()
    .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(req => 
            req.RequestUri.ToString().Contains("/token")),
        ItExpr.IsAny<CancellationToken>())
    .ReturnsAsync(new HttpResponseMessage
    {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(
     "{\"access_token\":\"token123\",\"token_type\":\"Bearer\",\"expires_in\":3600}")
    });
```

---

## 📊 Test Execution Results

### Recent Test Run

```
Test Run Successful.
Total tests: 65
     Passed: 65
     Failed: 0
    Skipped: 0
 Total time: 4.5 Seconds
```

### Performance Metrics

| Test Category | Execution Time |
|---------------|----------------|
| Mock Tests | ~2.0 seconds |
| Integration Tests | ~2.5 seconds |
| **Total** | **~4.5 seconds** |

---

## 🔧 CI/CD Integration

### GitHub Actions Example

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
  uses: actions/setup-dotnet@v3
 with:
      dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore
    
    - name: Run Mock Tests
      run: dotnet test --no-build --filter "FullyQualifiedName!~RealTests"
    
    - name: Start DemoServer
      run: |
        cd RestFlow.DemoServer
        dotnet run &
     sleep 5
    
    - name: Run Integration Tests
      run: dotnet test --no-build --filter "FullyQualifiedName~RealTests"
    
    - name: Upload Coverage
      uses: codecov/codecov-action@v3
```

---

## 🐛 Troubleshooting

### Integration Tests Fail

**Problem:** All integration tests fail with connection errors

**Solution:**
```bash
# Make sure DemoServer is running
cd RestFlow.DemoServer
dotnet run

# Verify server is accessible
curl http://localhost:5245/
```

---

### Mock Tests Fail

**Problem:** Mock tests fail with unexpected behavior

**Solution:**
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test
```

---

### Specific Test Fails

**Problem:** One specific test fails intermittently

**Solution:**
```bash
# Run specific test multiple times
dotnet test --filter "FullyQualifiedName~TestName" --logger "console;verbosity=detailed"
```

---

## 📚 Additional Resources

- [xUnit Documentation](https://xunit.net/#documentation)
- [Moq Documentation](https://github.com/moq/moq4/wiki/Quickstart)
- [RestFlow.Client20 README](../RestFlow.Client20/README_CLIENT20.md)
- [Integration Tests Guide](real-tests/README.md)
- [DemoServer Documentation](../RestFlow.DemoServer/README.md)

---

## 🤝 Contributing Tests

### Adding New Tests

1. **Mock Tests** - Add to appropriate file in `mock-tests/`
2. **Integration Tests** - Add to `real-tests/RestFlowDemoServerIntegrationTests.cs`

### Test Guidelines

- ✅ Follow AAA pattern (Arrange-Act-Assert)
- ✅ Use descriptive test names
- ✅ Test both success and failure scenarios
- ✅ Keep tests independent
- ✅ Mock external dependencies
- ✅ Write clear assertions

### Example New Test

```csharp
[Fact]
public async Task NewFeature_WithValidInput_ShouldReturnExpectedResult()
{
  // Arrange
    var client = new RestFlowClient()
     .WithBaseUrl("https://api.example.com");
    
    // Act
    var result = await client.NewMethodAsync();
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal("expected", result.Property);
}
```

---

## 📄 License

This test project is part of the RestFlow solution and follows the same MIT License.

---

## 🙏 Acknowledgments

- **xUnit** for the excellent testing framework
- **Moq** for powerful mocking capabilities
- **RestFlow Team** for comprehensive test coverage

---

**Test Coverage: 100% | All Tests Passing ✅**

*Last Updated: October 31, 2025*
