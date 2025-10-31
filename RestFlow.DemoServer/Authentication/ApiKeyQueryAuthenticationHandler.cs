using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RestFlow.DemoServer.Authentication;

public class ApiKeyQueryAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // Hardcoded API key
    private const string ValidApiKey = "a-static-api-key";
    private const string QueryParamName = "api_key";

    public ApiKeyQueryAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Query.ContainsKey(QueryParamName))
        {
            return Task.FromResult(AuthenticateResult.Fail($"Missing {QueryParamName} query parameter"));
        }

        var apiKey = Request.Query[QueryParamName].ToString();

        if (apiKey == ValidApiKey)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, "api-key-query-user") };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
    }
}
