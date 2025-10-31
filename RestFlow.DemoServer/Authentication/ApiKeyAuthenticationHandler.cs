using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RestFlow.DemoServer.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // Hardcoded API key
    private const string ValidApiKey = "a-static-api-key";
    private const string HeaderName = "X-API-KEY";

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(HeaderName))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Missing {HeaderName} Header"));
        }

        var apiKey = Request.Headers[HeaderName].ToString();

        if (apiKey == ValidApiKey)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "api-key-user") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
    }
}
