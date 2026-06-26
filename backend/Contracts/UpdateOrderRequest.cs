// Application layer
using System.ComponentModel.DataAnnotations;
using ProductApi.Models;

namespace ProductApi.Contracts;

public class UpdateOrderRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public OrderStatus Status { get; set; }

    [Required]
    public int Version { get; set; }

    [MaxLength(50)]
    public string? Awb { get; set; }
}
