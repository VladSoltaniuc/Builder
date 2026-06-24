// Application layer
namespace ProductApi.Contracts;

public record OrderResponse(int Id, int UserId, string UserName, int ProductId, string ProductName, int Quantity, decimal TotalPrice, string Status, DateTime CreatedAt, int Version);
