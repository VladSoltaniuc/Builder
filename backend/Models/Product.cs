// Domain layer
namespace ProductApi.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ProductCategory Category { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int Version { get; set; } = 1;

    public string? ImagePath { get; set; }
}

public enum ProductCategory
{
    Peripherals  = 0,
    Monitors     = 1,
    Storage      = 2,
    Audio        = 3,
    Accessories  = 4,
    Furniture    = 5,
    Lighting     = 6
}
