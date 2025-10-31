using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace RestFlow.DemoServer.Authentication;

public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    // Hardcoded credentials
    private const string Username = "admin";
    private const string Password = "password";

    public BasicAuthenticationHandler(
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
            if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var credentialBytes = Convert.FromBase64String(encodedCredentials);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

            if (credentials.Length != 2)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Credentials Format"));
            }

            var username = credentials[0];
            var password = credentials[1];

            if (username == Username && password == Password)
            {
                var claims = new[] { new Claim(ClaimTypes.Name, username) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }

            return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }
    }
}
