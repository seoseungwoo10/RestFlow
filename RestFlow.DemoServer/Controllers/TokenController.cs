using Microsoft.AspNetCore.Mvc;
using RestFlow.DemoServer.Services;

namespace RestFlow.DemoServer.Controllers;

[ApiController]
[Route("[controller]")]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    // Hardcoded credentials
    private const string ClientId = "restflow-client";
    private const string ClientSecret = "restflow-secret";
    private const string Username = "user";
    private const string UserPassword = "pass";
    private const string AuthCode = "static-auth-code-for-testing";
    private const string RefreshTokenPassword = "static-refresh-token-for-password-grant";
    private const string RefreshTokenAuthCode = "static-refresh-token-for-auth-code-grant";

    public TokenController(TokenService tokenService, JwtSettings jwtSettings)
    {
        _tokenService = tokenService;
        _jwtSettings = jwtSettings;
    }

    [HttpPost]
    [Consumes("application/x-www-form-urlencoded")]
    public IActionResult Post([FromForm] TokenRequest request)
    {
        try
        {
            return request.GrantType switch
            {
                "client_credentials" => HandleClientCredentials(request),
                "password" => HandlePassword(request),
                "authorization_code" => HandleAuthorizationCode(request),
                "refresh_token" => HandleRefreshToken(request),
                _ => BadRequest(new { error = "unsupported_grant_type", error_description = "The grant type is not supported" })
            };
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "invalid_request", error_description = ex.Message });
        }
    }

    private IActionResult HandleClientCredentials(TokenRequest request)
    {
        if (request.ClientId != ClientId || request.ClientSecret != ClientSecret)
        {
            return Unauthorized(new { error = "invalid_client", error_description = "Client authentication failed" });
        }

        var scopes = string.IsNullOrEmpty(request.Scope) 
            ? new List<string> { "read", "write" }
            : request.Scope.Split(' ').ToList();

        var accessToken = _tokenService.GenerateAccessToken("client_credentials_user", "client_credentials", scopes);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationSeconds,
            Scope = string.Join(" ", scopes)
        });
    }

    private IActionResult HandlePassword(TokenRequest request)
    {
        if (request.Username != Username || request.Password != UserPassword)
        {
            return Unauthorized(new { error = "invalid_grant", error_description = "Invalid username or password" });
        }

        if (request.ClientId != ClientId || request.ClientSecret != ClientSecret)
        {
            return Unauthorized(new { error = "invalid_client", error_description = "Client authentication failed" });
        }

        var scopes = string.IsNullOrEmpty(request.Scope)
            ? new List<string> { "read", "write" }
            : request.Scope.Split(' ').ToList();

        var accessToken = _tokenService.GenerateAccessToken(request.Username!, "password", scopes);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationSeconds,
            RefreshToken = RefreshTokenPassword,
            Scope = string.Join(" ", scopes)
        });
    }

    private IActionResult HandleAuthorizationCode(TokenRequest request)
    {
        if (request.Code != AuthCode)
        {
            return Unauthorized(new { error = "invalid_grant", error_description = "Invalid authorization code" });
        }

        if (request.ClientId != ClientId || request.ClientSecret != ClientSecret)
        {
            return Unauthorized(new { error = "invalid_client", error_description = "Client authentication failed" });
        }

        var scopes = new List<string> { "read", "write" };
        var accessToken = _tokenService.GenerateAccessToken("auth_code_user", "authorization_code", scopes);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationSeconds,
            RefreshToken = RefreshTokenAuthCode,
            Scope = string.Join(" ", scopes)
        });
    }

    private IActionResult HandleRefreshToken(TokenRequest request)
    {
        if (request.RefreshToken != RefreshTokenPassword && request.RefreshToken != RefreshTokenAuthCode)
        {
            return Unauthorized(new { error = "invalid_grant", error_description = "Invalid refresh token" });
        }

        // Determine grant type based on refresh token
        var grantType = request.RefreshToken == RefreshTokenPassword ? "password" : "authorization_code";
        var subject = request.RefreshToken == RefreshTokenPassword ? "user" : "auth_code_user";

        var scopes = new List<string> { "read", "write" };
        var accessToken = _tokenService.GenerateAccessToken(subject, grantType, scopes);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = _jwtSettings.ExpirationSeconds,
            RefreshToken = request.RefreshToken!, // Return the same refresh token
            Scope = string.Join(" ", scopes)
        });
    }
}

public class TokenRequest
{
    [FromForm(Name = "grant_type")]
    public string? GrantType { get; set; }

    [FromForm(Name = "client_id")]
    public string? ClientId { get; set; }

    [FromForm(Name = "client_secret")]
    public string? ClientSecret { get; set; }

    [FromForm(Name = "username")]
    public string? Username { get; set; }

    [FromForm(Name = "password")]
    public string? Password { get; set; }

    [FromForm(Name = "scope")]
    public string? Scope { get; set; }

    [FromForm(Name = "code")]
    public string? Code { get; set; }

    [FromForm(Name = "redirect_uri")]
    public string? RedirectUri { get; set; }

    [FromForm(Name = "refresh_token")]
    public string? RefreshToken { get; set; }
}

public class TokenResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}
