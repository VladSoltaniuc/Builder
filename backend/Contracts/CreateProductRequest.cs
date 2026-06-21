using System.ComponentModel.DataAnnotations;

namespace ProductApi.Contracts;

/// <summary>
/// DTO pentru crearea unui produs (POST). Nu conține Id - acela este generat de server.
/// Atributele [Required]/[Range] sunt validate automat de ASP.NET înainte să intre în controller
/// (pentru că folosim [ApiController]). Dacă validarea pică, clientul primește automat 400.
/// </summary>
public class CreateProductRequest
{
    [Required(ErrorMessage = "Numele este obligatoriu.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Numele trebuie să aibă între 2 și 100 de caractere.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Categoria este obligatorie.")]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Range(0.01, 1_000_000, ErrorMessage = "Prețul trebuie să fie mai mare ca 0.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stocul nu poate fi negativ.")]
    public int Stock { get; set; }
}
