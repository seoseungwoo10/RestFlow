# RestFlow DemoServer Integration Tests

This folder contains integration tests that test the `RestFlow.Client20` library against the live `RestFlow.DemoServer` API.

## Prerequisites

**IMPORTANT**: These tests require the `RestFlow.DemoServer` to be running locally before executing the tests.

### Starting the DemoServer

1. Open a terminal/command prompt
2. Navigate to the DemoServer directory:
   ```bash
   cd RestFlow.DemoServer
   ```
3. Run the server:
   ```bash
   dotnet run
   ```
4. Verify the server is running at: `http://localhost:5245`

## Test Coverage

The `RestFlowDemoServerIntegrationTests` class contains comprehensive tests for all authentication methods supported by RestFlow:

### 1. Health Check & Non-Auth Tests (2 tests)
- ✅ Health check endpoint (`/`)
- ✅ Non-authenticated endpoint (`/api/no-auth/test`)

### 2. Basic Authentication Tests (3 tests)
- ✅ Valid credentials
- ✅ Invalid credentials
- ✅ Missing credentials

### 3. Bearer Token Tests (3 tests)
- ✅ Valid token
- ✅ Invalid token
- ✅ Missing token

### 4. API Key Tests (4 tests)
- ✅ Valid API key in header
- ✅ Invalid API key in header
- ✅ Valid API key in query parameter
- ✅ Invalid API key in query parameter

### 5. OAuth 2.0 Client Credentials Tests (4 tests)
- ✅ Valid client credentials
- ✅ Invalid client ID
- ✅ Invalid client secret
- ✅ Token reuse across multiple requests

### 6. OAuth 2.0 Password Credentials Tests (3 tests)
- ✅ Valid username/password
- ✅ Invalid username
- ✅ Invalid password

### 7. OAuth 2.0 Authorization Code (Refresh Token) Tests (3 tests)
- ✅ Valid refresh token (auth code grant)
- ✅ Valid refresh token (password grant)
- ✅ Invalid refresh token

### 8. OAuth 2.0 Advanced Features Tests (1 test)
- ✅ Token expiration and auto-refresh

### 9. Error Tests (1 test)
- ✅ Protected resource without token

**Total: 24 integration tests**

## Running the Tests

### Option 1: Visual Studio Test Explorer
1. Build the solution
2. Open Test Explorer (Test → Test Explorer)
3. Make sure `RestFlow.DemoServer` is running
4. Run tests from the `RestFlowDemoServerIntegrationTests` class

### Option 2: Command Line
```bash
# Make sure DemoServer is running in another terminal first!
dotnet test RestFlow.Client20.Tests\RestFlow.Client20.Tests.csproj --filter "FullyQualifiedName~RestFlowDemoServerIntegrationTests"
```

### Option 3: Run All Tests (Mock + Integration)
```bash
dotnet test RestFlow.Client20.Tests\RestFlow.Client20.Tests.csproj
```

## Test Credentials

The tests use the hardcoded credentials from the DemoServer:

### Basic Authentication
- Username: `admin`
- Password: `password`

### Bearer Token
- Token: `a-static-bearer-token-for-testing`

### API Key
- Header Name: `X-API-KEY`
- Query Parameter: `api_key`
- Value: `a-static-api-key`

### OAuth 2.0
- Client ID: `restflow-client`
- Client Secret: `restflow-secret`
- Username: `user`
- Password: `pass`
- Auth Code: `static-auth-code-for-testing`
- Refresh Token (Password Grant): `static-refresh-token-for-password-grant`
- Refresh Token (Auth Code Grant): `static-refresh-token-for-auth-code-grant`

## Expected Test Results

When the DemoServer is running correctly, all 24 integration tests should **PASS**.

If the DemoServer is not running, all tests will **FAIL** with connection errors.

## Troubleshooting

### All tests fail with connection errors
- **Cause**: DemoServer is not running
- **Solution**: Start the DemoServer as described in the Prerequisites section

### Some tests fail with 401 Unauthorized
- **Cause**: Server credentials may have been changed
- **Solution**: Verify the credentials in the DemoServer match the ones in the tests

### Tests fail intermittently
- **Cause**: OAuth token caching or timing issues
- **Solution**: Restart the DemoServer between test runs

## Test Structure

Each test follows the Arrange-Act-Assert pattern:

```csharp
[Fact]
public async Task TestName_Scenario_ExpectedResult()
{
    // Arrange - Set up the client with authentication
    var client = new RestFlowClient(_httpClient)
        .WithBaseUrl(BaseUrl)
        .WithAuthentication(...);

    // Act - Call the API
    var response = await client.GetAsync<ResponseType>("/endpoint");

    // Assert - Verify the response
    Assert.NotNull(response);
    Assert.Equal("expected", response.Property);
}
```

## Integration with CI/CD

To run these tests in a CI/CD pipeline:

1. Start the DemoServer in the background:
   ```bash
 cd RestFlow.DemoServer
   dotnet run &
DEMOSERVER_PID=$!
   sleep 5  # Wait for server to start
   ```

2. Run the integration tests:
   ```bash
   dotnet test RestFlow.Client20.Tests\RestFlow.Client20.Tests.csproj --filter "FullyQualifiedName~RestFlowDemoServerIntegrationTests"
   ```

3. Stop the DemoServer:
   ```bash
 kill $DEMOSERVER_PID
   ```

## Notes

- These are **integration tests**, not unit tests. They test the entire flow from client to server.
- The tests use a shared `HttpClient` instance for efficiency.
- Each test is independent and can run in any order.
- The tests verify both success and error scenarios.
- OAuth 2.0 tests verify token caching and reuse behavior.
