// Application layer
using System.ComponentModel.DataAnnotations;
using ProductApi.Models;

namespace ProductApi.Contracts;

public class UpdateProductRequest
{
    [Required(ErrorMessage = "NAME_REQUIRED")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "NAME_LENGTH")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "CATEGORY_REQUIRED")]
    [EnumDataType(typeof(ProductCategory), ErrorMessage = "CATEGORY_INVALID")]
    public ProductCategory Category { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "PRICE_RANGE")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "STOCK_RANGE")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "VERSION_REQUIRED")]
    public int Version { get; set; }
}
