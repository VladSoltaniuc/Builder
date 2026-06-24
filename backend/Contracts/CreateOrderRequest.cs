// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class CreateOrderRequest
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
