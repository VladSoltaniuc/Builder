// Application layer
namespace ProductApi.Contracts;

public record UpdateOrderResult(OrderResponse? Order, bool IsConflict)
{
    public static UpdateOrderResult Success(OrderResponse order) => new(order, false);
    public static UpdateOrderResult NotFound() => new(null, false);
    public static UpdateOrderResult Conflict() => new(null, true);
}
