// Application layer
using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

public class CreateOrderRequest
{
    [Required(ErrorMessage = "USER_ID_REQUIRED")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "PRODUCT_ID_REQUIRED")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "QUANTITY_MIN")]
    public int Quantity { get; set; }
}
