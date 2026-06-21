namespace ProductApi.Models;

/// <summary>
/// Entitatea (modelul) de domeniu - cum arată un produs "intern", în serviciu.
/// Nu o expunem direct prin API; pentru asta folosim DTO-urile din folderul Contracts/.
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int Version { get; set; } = 1;
}
