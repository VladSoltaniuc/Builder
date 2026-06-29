// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class GoogleLoginRequest
{
    [Required(ErrorMessage = "GOOGLE_TOKEN_REQUIRED")]
    public string IdToken { get; set; } = string.Empty;
}
