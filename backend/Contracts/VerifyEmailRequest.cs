// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "Verification token is required.")]
    public string Token { get; set; } = string.Empty;
}
