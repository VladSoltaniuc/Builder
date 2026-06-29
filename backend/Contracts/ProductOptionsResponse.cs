// Application layer
using ProductApi.Models;

namespace ProductApi.Contracts;

public record ProductOptionsResponse(ProductCategory[] Categories);
