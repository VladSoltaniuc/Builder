// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class TwoFactorLoginRequest
{
    [Required(ErrorMessage = "TWO_FACTOR_TOKEN_REQUIRED")]
    public string TwoFactorToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "CODE_REQUIRED")]
    public string Code { get; set; } = string.Empty;
}
