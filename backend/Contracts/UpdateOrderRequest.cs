// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class UpdateOrderRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = string.Empty;

    [Required]
    public int Version { get; set; }
}
