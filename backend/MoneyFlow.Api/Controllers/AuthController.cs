using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyFlow.Api.Auth;
using MoneyFlow.Api.Common;
using MoneyFlow.Api.Dtos;
using MoneyFlow.Api.Services;

namespace MoneyFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService auth, ICurrentUserService currentUser) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Register(RegisterRequest request)
    {
        var data = await auth.RegisterAsync(request);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<UserDto>.Ok(data));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(LoginRequest request) =>
        Ok(ApiResponse<LoginResponse>.Ok(await auth.LoginAsync(request)));

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me() =>
        Ok(ApiResponse<UserDto>.Ok(await auth.MeAsync(currentUser.UserId)));

    [Authorize]
    [HttpPut("settings")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateSettings(UpdateSettingsRequest request) =>
        Ok(ApiResponse<UserDto>.Ok(await auth.UpdateSettingsAsync(currentUser.UserId, request)));
}
