namespace ProductApi.Contracts;

/// <summary>
/// DTO (Data Transfer Object) returnat clientului.
/// De ce un DTO separat de model? Ca să poți schimba modelul intern fără să strici
/// contractul public al API-ului. 'record' = tip imutabil, perfect pentru date read-only.
/// </summary>
public record ProductResponse(
    int Id,
    string Name,
    string Category,
    decimal Price,
    int Stock,
    int Version);
