// Application layer
using System.ComponentModel.DataAnnotations;
using ProductApi.Constants;

namespace ProductApi.Contracts;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Numele trebuie să aibă între 2 și 100 de caractere.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Categoria este obligatorie.")]
    [EnumDataType(typeof(ProductCategory))]
    public ProductCategory Category { get; set; }

    [Range(0.01, 1_000_000, ErrorMessage = "Prețul trebuie să fie mai mare ca 0.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stocul nu poate fi negativ.")]
    public int Stock { get; set; }
}
