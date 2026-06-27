// Presentation layer
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Contracts;
using ProductApi.Services;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        => Ok(await authService.Register(request));

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        => Ok(await authService.Login(request));

    // Completes a login for accounts with 2FA enabled.
    [HttpPost("2fa/verify")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> VerifyTwoFactor(TwoFactorLoginRequest request)
        => Ok(await authService.VerifyTwoFactorLogin(request));

    // Begins 2FA enrollment for the signed-in user: returns the secret + QR URI.
    [Authorize]
    [HttpPost("2fa/setup")]
    [ProducesResponseType(typeof(TwoFactorSetupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TwoFactorSetupResponse>> SetupTwoFactor()
        => Ok(await authService.SetupTwoFactor(CurrentUserId()));

    // Confirms a code from the authenticator app and turns 2FA on.
    [Authorize]
    [HttpPost("2fa/enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableTwoFactor(TwoFactorCodeRequest request)
    {
        await authService.EnableTwoFactor(CurrentUserId(), request.Code);
        return NoContent();
    }

    // Turns 2FA off after verifying a current code.
    [Authorize]
    [HttpPost("2fa/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableTwoFactor(TwoFactorCodeRequest request)
    {
        await authService.DisableTwoFactor(CurrentUserId(), request.Code);
        return NoContent();
    }

    // Exchanges a Google id_token (from client-side Google Sign-In) for our own JWT,
    // creating or linking the local account as needed.
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Google(GoogleLoginRequest request)
        => Ok(await authService.LoginWithGoogle(request));

    // Echoes the identity carried by the bearer token — handy for the SPA to
    // confirm a session and for verifying the JWT pipeline end to end.
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var name = User.FindFirstValue("name");
        var role = User.FindFirstValue(ClaimTypes.Role);
        return Ok(new { id, name, email, role });
    }

    // The authenticated user's id, parsed from the bearer token's subject claim.
    private int CurrentUserId()
        => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
