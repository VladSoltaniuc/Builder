// Application layer
namespace ProductApi.Contracts;

public record UpdateProductResult(ProductResponse? Product, bool IsConflict)
{
    public static UpdateProductResult Success(ProductResponse product) => new(product, false);
    public static UpdateProductResult NotFound() => new(null, false);
    public static UpdateProductResult Conflict() => new(null, true);
}
