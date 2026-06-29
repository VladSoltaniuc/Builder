// Application layer
using System.ComponentModel.DataAnnotations;
using ProductApi.Models;

namespace ProductApi.Contracts;

public class UpdateOrderRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "QUANTITY_MIN")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "STATUS_REQUIRED")]
    public OrderStatus Status { get; set; }

    [Required(ErrorMessage = "VERSION_REQUIRED")]
    public int Version { get; set; }

    [MaxLength(50, ErrorMessage = "AWB_LENGTH")]
    public string? Awb { get; set; }
}
