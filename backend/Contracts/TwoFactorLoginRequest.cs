// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

// Completes a 2FA login: the pending token from /auth/login plus the TOTP code.
public class TwoFactorLoginRequest
{
    [Required(ErrorMessage = "Two-factor token is required.")]
    public string TwoFactorToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Code is required.")]
    public string Code { get; set; } = string.Empty;
}
