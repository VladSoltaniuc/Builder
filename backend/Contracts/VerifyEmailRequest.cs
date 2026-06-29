// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "VERIFICATION_TOKEN_REQUIRED")]
    public string Token { get; set; } = string.Empty;
}
