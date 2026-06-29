// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class LoginRequest
{
    [Required(ErrorMessage = "EMAIL_REQUIRED")]
    [EmailAddress(ErrorMessage = "EMAIL_INVALID")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "PASSWORD_REQUIRED")]
    public string Password { get; set; } = string.Empty;
}
