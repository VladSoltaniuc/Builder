// Application layer
using ProductApi.Constants;

namespace ProductApi.Contracts;

public record OrderResponse(int Id, int UserId, string UserName, int ProductId, string ProductName, int Quantity, decimal TotalPrice, OrderStatus Status, DateTime CreatedAt, int Version, string? InvoiceUrl, string? Awb);
