// Application layer
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProductApi.Models;

namespace ProductApi.Auth;

public class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private const string PurposeClaim = "purpose";
    private const string TwoFactorPurpose = "2fa";
    private static readonly TimeSpan PendingTokenLifetime = TimeSpan.FromMinutes(5);

    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return (Write(claims, expiresAt), expiresAt);
    }

    public string CreatePendingTwoFactorToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(PurposeClaim, TwoFactorPurpose),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        return Write(claims, DateTime.UtcNow.Add(PendingTokenLifetime));
    }

    public int? ReadPendingTwoFactorUserId(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = key,
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            if (principal.FindFirstValue(PurposeClaim) != TwoFactorPurpose)
                return null;
            var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(sub, out var id) ? id : null;
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }

    private string Write(IEnumerable<Claim> claims, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
