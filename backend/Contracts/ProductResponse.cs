// Application layer
namespace ProductApi.Contracts;

public record ProductResponse(
    int Id,
    string Name,
    string Category,
    decimal Price,
    int Stock,
    int Version,
    string? ImageUrl);
