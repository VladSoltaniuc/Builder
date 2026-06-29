// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class TwoFactorCodeRequest
{
    [Required(ErrorMessage = "CODE_REQUIRED")]
    public string Code { get; set; } = string.Empty;
}
