// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class GoogleLoginRequest
{
    // The id_token returned by Google Sign-In on the client.
    [Required(ErrorMessage = "Google id token is required.")]
    public string IdToken { get; set; } = string.Empty;
}
