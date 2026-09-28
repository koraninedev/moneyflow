using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MoneyFlow.Api.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) IssueToken(int userId, string email);
}

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    private readonly string _secret = configuration["Jwt:Secret"]
        ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? "MoneyFlow";
    private readonly int _expiryDays = int.TryParse(configuration["Jwt:ExpiryDays"], out var d) ? d : 7;

    public (string Token, DateTime ExpiresAt) IssueToken(int userId, string email)
    {
        var expiresAt = DateTime.UtcNow.AddDays(_expiryDays);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer: _issuer, audience: _issuer, claims: claims, expires: expiresAt, signingCredentials: credentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
