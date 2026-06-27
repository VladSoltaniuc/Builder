// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

// Used to confirm enabling or disabling 2FA for the signed-in user.
public class TwoFactorCodeRequest
{
    [Required(ErrorMessage = "Code is required.")]
    public string Code { get; set; } = string.Empty;
}
