using Automantri.Application.Auth;
using Automantri.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Automantri.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    IAdminAuthService authService,
    IOptions<AdminAuthOptions> options) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResultDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResultDto> Login([FromBody] LoginRequestDto request)
    {
        var result = authService.Login(request);
        return result is null ? Unauthorized(new { message = "Invalid admin credentials." }) : Ok(result);
    }

    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            username = User.Identity?.Name ?? options.Value.Username,
            role = "Admin",
        });
    }
}
