using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Ports.Security;
using Application.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

public class JwtTokenService : ITokenService
{
    public const string UsernameClaim = "username";

    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<AuthOptions> options)
    {
        _options = options.Value.Jwt;
    }

    public AccessToken Generate(Guid subject, string role, string name, string username)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, subject.ToString()),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, name),
            new(UsernameClaim, username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return new AccessToken(tokenString, expiresAt);
    }
}
