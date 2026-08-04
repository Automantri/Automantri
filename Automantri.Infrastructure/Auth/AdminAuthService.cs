using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Automantri.Application.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Automantri.Infrastructure.Auth;

public interface IAdminAuthService
{
    LoginResultDto? Login(LoginRequestDto request);
}

public sealed class AdminAuthService(IOptions<AdminAuthOptions> options) : IAdminAuthService
{
    private readonly AdminAuthOptions _options = options.Value;

    public LoginResultDto? Login(LoginRequestDto request)
    {
        if (!string.Equals(request.Username?.Trim(), _options.Username, StringComparison.Ordinal) ||
            !string.Equals(request.Password, _options.Password, StringComparison.Ordinal))
        {
            return null;
        }

        var expires = DateTimeOffset.UtcNow.AddHours(Math.Clamp(_options.TokenLifetimeHours, 1, 72));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, _options.Username),
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(JwtRegisteredClaimNames.Sub, _options.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return new LoginResultDto(
            new JwtSecurityTokenHandler().WriteToken(token),
            _options.Username,
            "Admin",
            expires);
    }
}
