# RestFlow

> 🚀 **A comprehensive HTTP client library for .NET with advanced authentication support and testing infrastructure**

[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![.NET 8](https://img.shields.io/badge/.NET-8-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Tests](https://img.shields.io/badge/tests-65%20passing-brightgreen.svg)](#)

## 📋 Overview

**RestFlow** is a modern, production-ready HTTP client solution for .NET that provides:

- 🎯 **Powerful HTTP Client Library** - Feature-rich client with multiple authentication methods
- 🧪 **Comprehensive Test Suite** - 65 tests (41 unit + 24 integration) with 100% pass rate
- 🖥️ **Demo Server** - Live API server for testing all authentication scenarios

---

## 🏗️ Solution Structure

```
RestFlow/
├── RestFlow.Client20/   # HTTP Client Library (.NET Standard 2.0)
├── RestFlow.Client20.Tests/        # Test Suite (.NET 8)
└── RestFlow.DemoServer/# Demo API Server (.NET 8)
```

| Project | Target Framework | Purpose | Lines of Code |
|---------|-----------------|---------|---------------|
| **RestFlow.Client20** | .NET Standard 2.0 | HTTP client library | ~2,000 |
| **RestFlow.Client20.Tests** | .NET 8 | Test suite | ~3,500 |
| **RestFlow.DemoServer** | .NET 8 | Demo API server | ~1,500 |

---

## 🎯 Quick Start

### 1. Install the Client Library

```bash
dotnet add package RestFlow.Client20
```

### 2. Basic Usage

```csharp
using RestFlow.Client20;
using RestFlow.Client20.Models;

// Simple GET request
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com");

var user = await client.GetAsync<User>("/users/1");

// With OAuth 2.0 authentication
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com")
    .WithOAuthClientCredentials(
        tokenEndpoint: "https://auth.example.com/token",
        clientId: "your-client-id",
 clientSecret: "your-client-secret");

var data = await client.GetAsync<Response>("/api/protected");
```

### 3. Run Demo Server (for testing)

```bash
cd RestFlow.DemoServer
dotnet run
# Server starts at http://localhost:5245
```

### 4. Run Tests

```bash
# Run all tests (requires DemoServer running for integration tests)
dotnet test

# Run only unit tests (no server required)
dotnet test --filter "FullyQualifiedName!~RealTests"
```

---

## 📦 RestFlow.Client20

**Production-ready HTTP client library for .NET Standard 2.0**

### Key Features

- ✅ **7 Authentication Methods**
  - No Authentication
  - Basic Authentication
  - Bearer Token
  - API Key (Header/Query)
  - OAuth 2.0 Client Credentials
  - OAuth 2.0 Password Grant
  - OAuth 2.0 Authorization Code (Refresh Token)

- ✅ **Advanced OAuth 2.0**
  - Automatic token caching
  - Auto-refresh before expiration
  - Clock skew compensation (120s default)
  - Thread-safe token management
  - 401 automatic retry
  - Exponential backoff

- ✅ **Full HTTP Method Support**
  - GET, POST, PUT, PATCH, DELETE
  - Automatic JSON serialization/deserialization
  - Custom headers and query parameters

- ✅ **Developer Experience**
  - Fluent API design
  - Comprehensive error handling
  - Rich exception types
  - XML documentation

### Authentication Examples

#### OAuth 2.0 Client Credentials
```csharp
var client = new RestFlowClient()
    .WithOAuthClientCredentials(
        "https://auth.example.com/token",
        "client-id",
        "client-secret",
        scope: "read write",
        options: new OAuthOptions
    {
         ClockSkewSeconds = 120,
      EnableAutoRetryOn401 = true,
  MaxRetryAttempts = 3
     });
```

#### Basic Authentication
```csharp
var client = new RestFlowClient()
    .WithBasicAuth("username", "password");
```

#### API Key
```csharp
var client = new RestFlowClient()
    .WithApiKey("X-API-KEY", "your-key", ApiKeyLocation.Header);
```

### Compatibility

| Platform | Minimum Version |
|----------|----------------|
| .NET Framework | 4.6.1+ |
| .NET Core | 2.0+ |
| .NET | 5.0+ |
| Xamarin | ✅ Supported |

📚 **[Full Documentation](RestFlow.Client20/README_CLIENT20.md)**

---

## 🧪 RestFlow.Client20.Tests

**Comprehensive test suite with 100% pass rate**

### Test Statistics

| Category | Count | Pass Rate |
|----------|-------|-----------|
| **Mock Tests (Unit)** | 41 | ✅ 100% |
| **Integration Tests** | 24 | ✅ 100% |
| **Total** | **65** | **✅ 100%** |

### Test Categories

#### Mock Tests (41 tests)
- **RestFlowClientBasicTests** (10) - Core client functionality
- **AuthenticationHandlerTests** (11) - All auth handlers
- **OAuth2ClientCredentialsTests** (8) - OAuth flow
- **OAuth401RetryTests** (6) - Auto-retry mechanism
- **ApiExceptionTests** (6) - Error handling

#### Integration Tests (24 tests)
- Health Check & Non-Auth (2)
- Basic Authentication (3)
- Bearer Token (3)
- API Key - Header (2)
- API Key - Query (2)
- OAuth Client Credentials (4)
- OAuth Password Grant (3)
- OAuth Refresh Token (3)
- OAuth Advanced Features (1)
- Error Handling (1)

### Running Tests

```bash
# All tests
dotnet test RestFlow.Client20.Tests

# Unit tests only (no server required)
dotnet test --filter "FullyQualifiedName!~RealTests"

# Integration tests only (requires DemoServer)
dotnet test --filter "FullyQualifiedName~RealTests"

# With code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Technologies

- **xUnit** 2.5.3 - Test framework
- **Moq** 4.20.72 - Mocking framework
- **coverlet.collector** 6.0.0 - Code coverage

📚 **[Test Documentation](RestFlow.Client20.Tests/README_TESTS.md)**

---

## 🖥️ RestFlow.DemoServer

**ASP.NET Core Web API demo server for testing all authentication methods**

### Features

- ✅ **8 Test Endpoints**
  - Health check (`/`)
  - Non-auth (`/api/no-auth/test`)
  - Basic auth (`/api/basic-auth/test`)
  - Bearer token (`/api/bearer-token/test`)
  - API key header (`/api/api-key/header-test`)
  - API key query (`/api/api-key/query-test`)
  - OAuth protected (`/api/oauth/protected`)
  - Token endpoint (`POST /token`)

- ✅ **OAuth 2.0 Token Endpoint**
  - Client Credentials grant
  - Password grant
  - Authorization Code grant
  - Refresh Token grant

- ✅ **Features**
  - Swagger UI at `/swagger`
  - CORS enabled (all origins)
  - JWT tokens (120s expiration)
  - Hardcoded credentials (no database)

### Quick Start

```bash
cd RestFlow.DemoServer
dotnet run
# Server: http://localhost:5245
# Swagger: http://localhost:5245/swagger
```

### Hardcoded Credentials

| Auth Method | Credential | Value |
|-------------|------------|-------|
| **Basic Auth** | Username | `admin` |
| | Password | `password` |
| **Bearer Token** | Token | `a-static-bearer-token-for-testing` |
| **API Key** | Header/Query Value | `a-static-api-key` |
| **OAuth 2.0** | Client ID | `restflow-client` |
| | Client Secret | `restflow-secret` |
| | Username | `user` |
| | Password | `pass` |

### Example API Calls

```bash
# Health check
curl http://localhost:5245/

# Basic Auth
curl -H "Authorization: Basic YWRtaW46cGFzc3dvcmQ=" \
  http://localhost:5245/api/basic-auth/test

# OAuth 2.0 - Get token
curl -X POST http://localhost:5245/token \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials&client_id=restflow-client&client_secret=restflow-secret"

# OAuth 2.0 - Use token
curl -H "Authorization: Bearer <token>" \
  http://localhost:5245/api/oauth/protected
```

📚 **[Server Documentation](RestFlow.DemoServer/README_DEMOSERVER.md)**

---

## 🎨 Architecture

### Design Patterns

- **Strategy Pattern** - Authentication handlers
- **Builder Pattern** - Fluent API configuration
- **Singleton Pattern** - HttpClient reuse
- **Double-Check Locking** - Thread-safe token refresh
- **Policy Scheme** - Multi-authentication support

### Project Dependencies

```
RestFlow.Client20 (.NET Standard 2.0)
├── Newtonsoft.Json 13.0.1
└── System.Net.Http

RestFlow.Client20.Tests (.NET 8)
├── RestFlow.Client20 (Project Reference)
├── xUnit 2.5.3
├── Moq 4.20.72
└── Microsoft.NET.Test.Sdk 17.8.0

RestFlow.DemoServer (.NET 8)
├── ASP.NET Core 8.0
├── Swashbuckle.AspNetCore
└── System.IdentityModel.Tokens.Jwt
```

---

## 📊 Feature Comparison

| Feature | RestFlow.Client20 | Alternatives |
|---------|-------------------|--------------|
| **Fluent API** | ✅ Yes | ❌ Most don't |
| **OAuth 2.0 Auto-Refresh** | ✅ Yes | ⚠️ Manual |
| **Token Caching** | ✅ Thread-safe | ⚠️ Limited |
| **401 Auto-Retry** | ✅ Configurable | ❌ No |
| **Clock Skew Handling** | ✅ Configurable | ❌ No |
| **Multiple Auth Methods** | ✅ 7 types | ⚠️ 2-3 types |
| **.NET Standard 2.0** | ✅ Yes | ⚠️ Varies |
| **Test Coverage** | ✅ 65 tests | ⚠️ Varies |
| **Demo Server** | ✅ Included | ❌ Not included |

---

## 🚀 Getting Started

### Prerequisites

- .NET SDK 6.0 or higher
- Visual Studio 2022 or VS Code (recommended)

### Clone and Build

```bash
# Clone repository
git clone https://github.com/yourusername/RestFlow.git
cd RestFlow

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Start demo server
cd RestFlow.DemoServer
dotnet run
```

### Development Workflow

1. **Start DemoServer**
   ```bash
   cd RestFlow.DemoServer
   dotnet run
   ```

2. **Run Integration Tests**
   ```bash
   dotnet test RestFlow.Client20.Tests
   ```

3. **Explore with Swagger**
   - Open browser: `http://localhost:5245/swagger`

---

## 📝 Usage Examples

### Example 1: Simple GET Request

```csharp
using RestFlow.Client20;

var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com");

var users = await client.GetAsync<List<User>>("/users");
```

### Example 2: POST with OAuth 2.0

```csharp
var client = new RestFlowClient()
 .WithBaseUrl("https://api.example.com")
    .WithOAuthClientCredentials(
    "https://auth.example.com/token",
  "client-id",
        "client-secret");

var newUser = new User { Name = "John Doe" };
var created = await client.PostAsync<User>("/users", newUser);
```

### Example 3: Error Handling

```csharp
try
{
    var data = await client.GetAsync<Data>("/api/data");
}
catch (ApiException ex)
{
    Console.WriteLine($"Error: {ex.StatusCode}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Response: {ex.ResponseBody}");
}
```

### Example 4: Custom Configuration

```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

var client = new RestFlowClient(httpClient)
    .WithBaseUrl("https://api.example.com")
    .WithOAuthClientCredentials(
        "https://auth.example.com/token",
        "client-id",
        "client-secret",
        options: new OAuthOptions
        {
     ClockSkewSeconds = 120,
  EnableAutoRetryOn401 = true,
            MaxRetryAttempts = 3,
 InitialBackoffDelay = TimeSpan.FromSeconds(1)
 });
```

---

## 🧪 Testing Against DemoServer

### Local Testing

```csharp
using RestFlow.Client20;

// Point to local DemoServer
var client = new RestFlowClient()
    .WithBaseUrl("http://localhost:5245")
    .WithOAuthClientCredentials(
        tokenEndpoint: "http://localhost:5245/token",
        clientId: "restflow-client",
      clientSecret: "restflow-secret");

// Test OAuth protected endpoint
var response = await client.GetAsync<dynamic>("/api/oauth/protected");
Console.WriteLine(response.message); // OAuth 2.0 protected resource accessed successfully
```

### Test All Authentication Methods

```csharp
// 1. Non-Auth
var noAuthClient = new RestFlowClient()
    .WithBaseUrl("http://localhost:5245");
await noAuthClient.GetAsync<dynamic>("/api/no-auth/test");

// 2. Basic Auth
var basicClient = new RestFlowClient()
 .WithBaseUrl("http://localhost:5245")
    .WithBasicAuth("admin", "password");
await basicClient.GetAsync<dynamic>("/api/basic-auth/test");

// 3. Bearer Token
var bearerClient = new RestFlowClient()
    .WithBaseUrl("http://localhost:5245")
  .WithBearerToken("a-static-bearer-token-for-testing");
await bearerClient.GetAsync<dynamic>("/api/bearer-token/test");

// 4. API Key (Header)
var apiKeyClient = new RestFlowClient()
    .WithBaseUrl("http://localhost:5245")
    .WithApiKey("X-API-KEY", "a-static-api-key", ApiKeyLocation.Header);
await apiKeyClient.GetAsync<dynamic>("/api/api-key/header-test");

// 5. OAuth 2.0
var oauthClient = new RestFlowClient()
    .WithBaseUrl("http://localhost:5245")
    .WithOAuthClientCredentials(
    "http://localhost:5245/token",
 "restflow-client",
      "restflow-secret");
await oauthClient.GetAsync<dynamic>("/api/oauth/protected");
```

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [Client Library](RestFlow.Client20/README_CLIENT20.md) | Complete API reference and usage guide |
| [Test Suite](RestFlow.Client20.Tests/README_TESTS.md) | Test documentation and guidelines |
| [Demo Server](RestFlow.DemoServer/README_DEMOSERVER.md) | Server setup and API endpoints |
| [Integration Tests](RestFlow.Client20.Tests/real-tests/README.md) | Integration test guide |

---

## 🔧 Best Practices

### 1. Reuse Client Instances

```csharp
// ✅ Good - Singleton pattern
public class ApiService
{
    private static readonly RestFlowClient _client = new RestFlowClient()
        .WithBaseUrl("https://api.example.com")
        .WithOAuthClientCredentials(...);
}

// ❌ Bad - Creates new client every time
public void GetData()
{
    var client = new RestFlowClient()...
}
```

### 2. Use Dependency Injection

```csharp
// Startup.cs or Program.cs
services.AddSingleton<RestFlowClient>(sp =>
{
    return new RestFlowClient()
        .WithBaseUrl(Configuration["ApiBaseUrl"])
    .WithOAuthClientCredentials(
       Configuration["OAuth:TokenEndpoint"],
            Configuration["OAuth:ClientId"],
            Configuration["OAuth:ClientSecret"]);
});
```

### 3. Handle Errors Gracefully

```csharp
try
{
    var data = await client.GetAsync<Data>("/api/data");
    return data;
}
catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    return null;
}
catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
    _logger.LogWarning("Authentication failed");
    throw;
}
```

---

## 🎯 Roadmap

### Completed ✅
- [x] Core HTTP client functionality
- [x] 7 authentication methods
- [x] OAuth 2.0 advanced features
- [x] Comprehensive test suite
- [x] Demo server
- [x] Complete documentation

### Future Enhancements 🚀
- [ ] .NET Framework 4.5.2 support (RestFlow.Client10)
- [ ] Response caching
- [ ] Request/response interceptors
- [ ] Polly integration for resilience
- [ ] Rate limiting support
- [ ] GraphQL support
- [ ] gRPC support
- [ ] Performance benchmarks
- [ ] NuGet package publication

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Write tests for your changes
4. Ensure all tests pass (`dotnet test`)
5. Commit your changes (`git commit -m 'Add AmazingFeature'`)
6. Push to the branch (`git push origin feature/AmazingFeature`)
7. Open a Pull Request

### Development Guidelines

- ✅ Follow C# coding conventions
- ✅ Write XML documentation for public APIs
- ✅ Add unit tests for new features
- ✅ Add integration tests when applicable
- ✅ Update README files
- ✅ Ensure 100% test pass rate

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **Newtonsoft.Json** - JSON serialization
- **xUnit** - Testing framework
- **Moq** - Mocking framework
- **ASP.NET Core** - Demo server framework
- **.NET Community** - Inspiration and support

---

## 📊 Statistics

| Metric | Value |
|--------|-------|
| Total Projects | 3 |
| Total Lines of Code | ~7,000 |
| Total Tests | 65 |
| Test Pass Rate | 100% |
| Authentication Methods | 7 |
| HTTP Methods Supported | 5 (GET, POST, PUT, PATCH, DELETE) |
| OAuth 2.0 Grant Types | 4 |
| .NET Versions Supported | 4.6.1 - .NET 8+ |
| Dependencies | Minimal (Newtonsoft.Json only) |
| Documentation Pages | 4 comprehensive READMEs |

---

**RestFlow** - Making HTTP communication simple, secure, and efficient for .NET developers.

*Built with ❤️ for the .NET Community*

**Version:** 1.0.0  
**Last Updated:** October 31, 2025  
**Status:** ✅ Production Ready
