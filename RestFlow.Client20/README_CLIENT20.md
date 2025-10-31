# RestFlow.Client20

> 🚀 **A powerful and flexible HTTP client library for .NET Standard 2.0 with comprehensive authentication support**

[![.NET Standard 2.0](https://img.shields.io/badge/.NET%20Standard-2.0-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![NuGet](https://img.shields.io/badge/NuGet-v1.0.0-green.svg)](https://www.nuget.org/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 📋 Overview

**RestFlow.Client20** is a modern, production-ready HTTP client library for .NET applications that simplifies REST API communication with built-in support for multiple authentication methods, including advanced OAuth 2.0 flows.

### Key Features

- ✅ **Fluent API Design** - Intuitive method chaining for easy configuration
- ✅ **Multiple Authentication Methods** - 7 authentication strategies out of the box
- ✅ **Production-Grade OAuth 2.0** - Advanced token management with caching and auto-refresh
- ✅ **Automatic JSON Serialization** - Seamless object mapping with Newtonsoft.Json
- ✅ **Comprehensive HTTP Methods** - GET, POST, PUT, PATCH, DELETE support
- ✅ **Thread-Safe** - Concurrent request handling with proper synchronization
- ✅ **Error Handling** - Rich exception types with detailed error information
- ✅ **.NET Standard 2.0** - Compatible with .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5+

---

## 🎯 Quick Start

### Installation

```bash
dotnet add package RestFlow.Client20
```

Or via NuGet Package Manager:
```bash
Install-Package RestFlow.Client20
```

### Basic Usage

```csharp
using RestFlow.Client20;

// Create client and make a GET request
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com");

var user = await client.GetAsync<User>("/users/1");
```

---

## 🔐 Authentication Methods

RestFlow.Client20 supports 7 different authentication strategies:

### 1. No Authentication

```csharp
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com");

var data = await client.GetAsync<Response>("/public/data");
```

### 2. Basic Authentication

```csharp
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com")
    .WithBasicAuth("username", "password");

var data = await client.GetAsync<Response>("/protected/resource");
```

### 3. Bearer Token

```csharp
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com")
    .WithBearerToken("your-access-token");

var data = await client.GetAsync<Response>("/api/data");
```

### 4. API Key (Header)

```csharp
using RestFlow.Client20.Models;

var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com")
    .WithApiKey("X-API-KEY", "your-api-key", ApiKeyLocation.Header);

var data = await client.GetAsync<Response>("/api/data");
```

### 5. API Key (Query Parameter)

```csharp
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com")
    .WithApiKey("api_key", "your-api-key", ApiKeyLocation.QueryParam);

var data = await client.GetAsync<Response>("/api/data");
```

### 6. OAuth 2.0 - Client Credentials Grant

```csharp
using RestFlow.Client20.Models;

var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com")
    .WithOAuthClientCredentials(
        tokenEndpoint: "https://auth.example.com/token",
        clientId: "your-client-id",
   clientSecret: "your-client-secret",
        scope: "read write",
        options: new OAuthOptions
        {
            ClockSkewSeconds = 120,
      EnableAutoRetryOn401 = true,
     MaxRetryAttempts = 3
      });

// Token is automatically acquired and refreshed
var data = await client.GetAsync<Response>("/api/protected");
```

### 7. OAuth 2.0 - Password Credentials Grant

```csharp
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com")
    .WithOAuthPasswordCredentials(
     tokenEndpoint: "https://auth.example.com/token",
      username: "user@example.com",
      password: "user-password",
        clientId: "your-client-id",
        clientSecret: "your-client-secret");

var data = await client.GetAsync<Response>("/api/user-data");
```

### 8. OAuth 2.0 - Authorization Code (Refresh Token Flow)

```csharp
var client = new RestFlowClient()
    .WithBaseUrl("https://api.example.com")
  .WithOAuthAuthorizationCode(
        tokenEndpoint: "https://auth.example.com/token",
        refreshToken: "your-refresh-token",
        clientId: "your-client-id",
  clientSecret: "your-client-secret");

var data = await client.GetAsync<Response>("/api/data");
```

---

## 🌐 HTTP Methods

RestFlow.Client20 supports all standard HTTP methods:

### GET Request

```csharp
// Get single object
var user = await client.GetAsync<User>("/users/123");

// Get collection
var users = await client.GetAsync<List<User>>("/users");
```

### POST Request

```csharp
var newUser = new User 
{ 
    Name = "John Doe", 
    Email = "john@example.com" 
};

// With response
var createdUser = await client.PostAsync<User>("/users", newUser);

// Without response
await client.PostAsync("/users", newUser);
```

### PUT Request

```csharp
var updatedUser = new User 
{ 
    Id = 123,
    Name = "Jane Doe", 
    Email = "jane@example.com" 
};

// With response
var result = await client.PutAsync<User>("/users/123", updatedUser);

// Without response
await client.PutAsync("/users/123", updatedUser);
```

### PATCH Request

```csharp
var partialUpdate = new { Email = "newemail@example.com" };

// With response
var result = await client.PatchAsync<User>("/users/123", partialUpdate);

// Without response
await client.PatchAsync("/users/123", partialUpdate);
```

### DELETE Request

```csharp
await client.DeleteAsync("/users/123");
```

---

## ⚙️ Advanced Configuration

### OAuth 2.0 Options

Customize OAuth behavior with `OAuthOptions`:

```csharp
var options = new OAuthOptions
{
    // Clock skew compensation (tokens refresh this many seconds before expiration)
    ClockSkewSeconds = 120,  // Default: 120 seconds
    
    // Maximum retry attempts for token refresh
    MaxRetryAttempts = 3,  // Default: 3
    
    // Initial backoff delay for retries
    InitialBackoffDelay = TimeSpan.FromSeconds(1),  // Default: 1 second
    
    // Enable automatic retry on 401 Unauthorized
    EnableAutoRetryOn401 = true,  // Default: true
    
// Custom time provider (useful for testing)
    TimeProvider = new SystemTimeProvider()  // Default: SystemTimeProvider
};

var client = new RestFlowClient()
    .WithOAuthClientCredentials(
  "https://auth.example.com/token",
        "client-id",
"client-secret",
        options: options);
```

### Custom HttpClient

```csharp
// Use your own HttpClient instance
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(30)
};

var client = new RestFlowClient(httpClient, disposeHttpClient: true)
    .WithBaseUrl("https://api.example.com");
```

### Custom Authentication Handler

Implement your own authentication strategy:

```csharp
using RestFlow.Client20.Handlers.Authentication;

public class CustomAuthHandler : IAuthenticationHandler
{
  public Task ApplyAsync(HttpRequestMessage request)
    {
        request.Headers.Add("X-Custom-Auth", "custom-value");
        return Task.CompletedTask;
    }
}

var client = new RestFlowClient()
    .WithAuthentication(new CustomAuthHandler());
```

---

## 🛡️ Error Handling

RestFlow.Client20 provides rich error information through `ApiException`:

```csharp
using RestFlow.Client20.Models;

try
{
    var data = await client.GetAsync<Response>("/api/data");
}
catch (ApiException ex)
{
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Response Body: {ex.ResponseBody}");
    
    // Access response headers
    foreach (var header in ex.Headers)
    {
        Console.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
    }
}
```

### Common Error Scenarios

```csharp
catch (ApiException ex)
{
    switch (ex.StatusCode)
    {
        case HttpStatusCode.Unauthorized:
        // Authentication failed
            break;
        case HttpStatusCode.Forbidden:
// Access denied
    break;
        case HttpStatusCode.NotFound:
      // Resource not found
  break;
        case HttpStatusCode.BadRequest:
// Invalid request
            break;
        default:
     // Other errors
            break;
  }
}
```

---

## 🔄 OAuth 2.0 Advanced Features

### 1. Automatic Token Caching

Tokens are automatically cached and reused across requests:

```csharp
var client = new RestFlowClient()
    .WithOAuthClientCredentials(
        "https://auth.example.com/token",
        "client-id",
   "client-secret");

// Token is acquired on first request
await client.GetAsync<Data>("/api/data1");

// Token is reused (not re-requested)
await client.GetAsync<Data>("/api/data2");
await client.GetAsync<Data>("/api/data3");
```

### 2. Automatic Token Refresh

Tokens are automatically refreshed before expiration:

```csharp
var options = new OAuthOptions
{
    ClockSkewSeconds = 120  // Refresh 120 seconds before expiration
};

var client = new RestFlowClient()
    .WithOAuthClientCredentials(
        "https://auth.example.com/token",
        "client-id",
        "client-secret",
   options: options);

// Token automatically refreshes when needed
await client.GetAsync<Data>("/api/data");
```

### 3. Automatic 401 Retry

Failed requests are automatically retried after token refresh:

```csharp
var options = new OAuthOptions
{
    EnableAutoRetryOn401 = true
};

var client = new RestFlowClient()
    .WithOAuthClientCredentials(
        "https://auth.example.com/token",
        "client-id",
        "client-secret",
        options: options);

// If request returns 401, token is refreshed and request is retried once
await client.GetAsync<Data>("/api/data");
```

### 4. Thread-Safe Token Management

Token acquisition and refresh are thread-safe:

```csharp
var client = new RestFlowClient()
    .WithOAuthClientCredentials(
 "https://auth.example.com/token",
        "client-id",
        "client-secret");

// Safe to call concurrently from multiple threads
var tasks = Enumerable.Range(0, 10)
.Select(_ => client.GetAsync<Data>("/api/data"));

await Task.WhenAll(tasks);
```

---

## 🏗️ Architecture

### Project Structure

```
RestFlow.Client20/
├── RestFlowClient.cs   # Main client class
├── Handlers/
│   └── Authentication/
│       ├── IAuthenticationHandler.cs          # Auth handler interface
│       ├── NoAuthHandler.cs          # No authentication
│       ├── BasicAuthHandler.cs         # Basic authentication
│       ├── BearerTokenAuthHandler.cs       # Bearer token
│       ├── ApiKeyAuthHandler.cs    # API key (header/query)
│  ├── IOAuthHandler.cs               # OAuth handler interface
│       ├── OAuth2ClientCredentialsHandler.cs  # OAuth 2.0 client credentials
│   ├── OAuth2PasswordCredentialsHandler.cs # OAuth 2.0 password
│       └── OAuth2AuthorizationCodeHandler.cs  # OAuth 2.0 auth code
└── Models/
    ├── ApiException.cs            # API exception class
    ├── ApiKeyLocation.cs         # API key location enum
    ├── OAuthOptions.cs          # OAuth configuration
    └── ITimeProvider.cs   # Time provider interface
```

### Design Patterns

- **Strategy Pattern**: Authentication handlers
- **Builder Pattern**: Fluent API configuration
- **Singleton Pattern**: HttpClient reuse
- **Double-Check Locking**: Thread-safe token refresh

---

## 📊 Compatibility

### Target Framework

- **.NET Standard 2.0**

### Compatible With

| Platform | Minimum Version |
|----------|----------------|
| .NET Framework | 4.6.1+ |
| .NET Core | 2.0+ |
| .NET | 5.0+ |
| Xamarin.iOS | 10.14+ |
| Xamarin.Android | 8.0+ |
| UWP | 10.0.16299+ |

### Dependencies

- **Newtonsoft.Json** >= 13.0.1

---

## 🧪 Testing

The library includes comprehensive test coverage:

- **41 Unit Tests** (100% passing)
- **24 Integration Tests** (with live DemoServer)

Run tests:
```bash
dotnet test RestFlow.Client20.Tests
```

---

## 📝 Examples

### Complete Example - OAuth 2.0 Client Credentials

```csharp
using System;
using System.Threading.Tasks;
using RestFlow.Client20;
using RestFlow.Client20.Models;

public class Program
{
    public static async Task Main()
    {
        // Configure client
        var client = new RestFlowClient()
            .WithBaseUrl("https://api.example.com")
            .WithOAuthClientCredentials(
          tokenEndpoint: "https://auth.example.com/token",
        clientId: "my-client-id",
       clientSecret: "my-client-secret",
       scope: "read write",
                options: new OAuthOptions
             {
   ClockSkewSeconds = 120,
      EnableAutoRetryOn401 = true,
       MaxRetryAttempts = 3
        });

        try
  {
        // GET request
var users = await client.GetAsync<List<User>>("/api/users");
  Console.WriteLine($"Found {users.Count} users");

       // POST request
            var newUser = new User 
         { 
             Name = "John Doe", 
     Email = "john@example.com" 
    };
  var created = await client.PostAsync<User>("/api/users", newUser);
            Console.WriteLine($"Created user with ID: {created.Id}");

// PUT request
        created.Name = "Jane Doe";
            await client.PutAsync($"/api/users/{created.Id}", created);
            Console.WriteLine("User updated");

       // DELETE request
            await client.DeleteAsync($"/api/users/{created.Id}");
   Console.WriteLine("User deleted");
      }
    catch (ApiException ex)
    {
            Console.WriteLine($"API Error: {ex.StatusCode} - {ex.Message}");
        Console.WriteLine($"Response: {ex.ResponseBody}");
        }
    }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

---

## 🔧 Best Practices

### 1. Reuse RestFlowClient Instances

```csharp
// ✅ Good - Reuse client
public class UserService
{
    private readonly RestFlowClient _client;
  
    public UserService()
    {
        _client = new RestFlowClient()
            .WithBaseUrl("https://api.example.com")
  .WithBearerToken("token");
    }
    
    public Task<User> GetUserAsync(int id) 
        => _client.GetAsync<User>($"/users/{id}");
}

// ❌ Bad - Create new client for each request
public Task<User> GetUserAsync(int id)
{
    var client = new RestFlowClient()
        .WithBaseUrl("https://api.example.com");
    return client.GetAsync<User>($"/users/{id}");
}
```

### 2. Use Dependency Injection

```csharp
// Startup.cs
services.AddSingleton(sp =>
{
    return new RestFlowClient()
        .WithBaseUrl("https://api.example.com")
        .WithOAuthClientCredentials(
       tokenEndpoint: "https://auth.example.com/token",
  clientId: Configuration["OAuth:ClientId"],
   clientSecret: Configuration["OAuth:ClientSecret"]);
});

// Usage
public class MyService
{
    private readonly RestFlowClient _client;
    
    public MyService(RestFlowClient client)
    {
        _client = client;
    }
}
```

### 3. Handle Errors Appropriately

```csharp
try
{
 var data = await client.GetAsync<Data>("/api/data");
    return data;
}
catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    // Handle not found
    return null;
}
catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
{
  // Handle unauthorized
    throw new UnauthorizedException("Authentication failed", ex);
}
catch (ApiException ex)
{
 // Handle other API errors
    _logger.LogError(ex, "API request failed");
  throw;
}
```

---

## 📚 Documentation

- [API Reference](docs/API_REFERENCE.md)
- [Authentication Guide](docs/AUTHENTICATION.md)
- [OAuth 2.0 Guide](docs/OAUTH_GUIDE.md)
- [Integration Tests](../RestFlow.Client20.Tests/real-tests/README.md)

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Built with ❤️ by the RestFlow Team
- Powered by [Newtonsoft.Json](https://www.newtonsoft.com/json)
- Inspired by modern REST client libraries

---

**Made with ❤️ for the .NET Community**
