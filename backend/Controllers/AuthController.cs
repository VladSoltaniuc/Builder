// Presentation layer
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
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request)
        => Ok(await authService.Register(request));

    // Confirms an emailed verification link, activating the account.
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
    {
        await authService.VerifyEmail(request.Token);
        return NoContent();
    }

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
        => Ok(await authService.SetupTwoFactor(GetCurrentUserId()));

    // Confirms a code from the authenticator app and turns 2FA on.
    [Authorize]
    [HttpPost("2fa/enable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnableTwoFactor(TwoFactorCodeRequest request)
    {
        await authService.EnableTwoFactor(GetCurrentUserId(), request.Code);
        return NoContent();
    }

    // Turns 2FA off after verifying a current code.
    [Authorize]
    [HttpPost("2fa/disable")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DisableTwoFactor(TwoFactorCodeRequest request)
    {
        await authService.DisableTwoFactor(GetCurrentUserId(), request.Code);
        return NoContent();
    }

    // Exchanges a Google id_token for JWT creating/linking the local account as needed
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Google(GoogleLoginRequest request)
        => Ok(await authService.LoginWithGoogle(request));

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileResponse>> Me()
        => Ok(await authService.GetProfile(GetCurrentUserId()));

}
