using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RestFlow.DemoServer.Services;

public class TokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public string GenerateAccessToken(string subject, string grantType, List<string>? scopes = null)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("grant_type", grantType)
        };

        if (scopes != null && scopes.Any())
        {
            claims.Add(new Claim("scope", string.Join(" ", scopes)));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(_jwtSettings.ExpirationSeconds),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
