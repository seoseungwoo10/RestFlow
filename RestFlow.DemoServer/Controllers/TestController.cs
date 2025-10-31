using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestFlow.DemoServer.Controllers;

[ApiController]
[Route("api")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Non-Auth test endpoint
    /// </summary>
    [HttpGet("no-auth/test")]
    [AllowAnonymous]
    public IActionResult NoAuthTest()
    {
        return Ok(new
        {
            status = "success",
            authType = "Non-Auth",
            message = "No authentication required",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Basic Auth test endpoint
    /// </summary>
    [HttpGet("basic-auth/test")]
    [Authorize(AuthenticationSchemes = "BasicAuth")]
    public IActionResult BasicAuthTest()
    {
        return Ok(new
        {
            status = "success",
            authType = "Basic Auth",
            user = User.Identity?.Name,
            message = "Basic authentication successful",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Static Bearer Token test endpoint
    /// </summary>
    [HttpGet("bearer-token/test")]
    [Authorize(AuthenticationSchemes = "StaticBearer")]
    public IActionResult BearerTokenTest()
    {
        return Ok(new
        {
            status = "success",
            authType = "Bearer Token",
            user = User.Identity?.Name,
            message = "Bearer token authentication successful",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// API Key (Header) test endpoint
    /// </summary>
    [HttpGet("api-key/header-test")]
    [Authorize(AuthenticationSchemes = "ApiKeyHeader")]
    public IActionResult ApiKeyHeaderTest()
    {
        return Ok(new
        {
            status = "success",
            authType = "API Key (Header)",
            user = User.Identity?.Name,
            message = "API key authentication (header) successful",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// API Key (Query) test endpoint
    /// </summary>
    [HttpGet("api-key/query-test")]
    [Authorize(AuthenticationSchemes = "ApiKeyQuery")]
    public IActionResult ApiKeyQueryTest()
    {
        return Ok(new
        {
            status = "success",
            authType = "API Key (Query)",
            user = User.Identity?.Name,
            message = "API key authentication (query) successful",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// OAuth 2.0 (JWT) protected endpoint
    /// </summary>
    [HttpGet("oauth/protected")]
    [Authorize(AuthenticationSchemes = "JwtBearer")]
    public IActionResult OAuthProtected()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        return Ok(new
        {
            status = "success",
            authType = "OAuth 2.0 (JWT)",
            user = User.Identity?.Name,
            claims = claims,
            message = "OAuth 2.0 JWT authentication successful",
            timestamp = DateTime.UtcNow
        });
    }
}
