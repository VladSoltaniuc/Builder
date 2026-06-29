// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class RegisterRequest
{
    [Required(ErrorMessage = "NAME_REQUIRED")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "NAME_LENGTH")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "EMAIL_REQUIRED")]
    [EmailAddress(ErrorMessage = "EMAIL_INVALID")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "PASSWORD_REQUIRED")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "PASSWORD_LENGTH")]
    public string Password { get; set; } = string.Empty;
}
