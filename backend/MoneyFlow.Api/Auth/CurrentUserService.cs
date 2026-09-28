using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MoneyFlow.Api.Common;

namespace MoneyFlow.Api.Auth;

public interface ICurrentUserService
{
    int UserId { get; }
    string Email { get; }
}

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public int UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (value is null || !int.TryParse(value, out var userId)) throw new UnauthorizedException();
            return userId;
        }
    }

    public string Email =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? throw new UnauthorizedException();
}
