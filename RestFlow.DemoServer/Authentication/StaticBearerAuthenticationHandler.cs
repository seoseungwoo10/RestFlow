using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace RestFlow.DemoServer.Authentication;

public class StaticBearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // Hardcoded static bearer token
    private const string ValidToken = "a-static-bearer-token-for-testing";

    public StaticBearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
        }

        try
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            if (token == ValidToken)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, "static-bearer-user") };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Invalid Token"));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }
    }
}
