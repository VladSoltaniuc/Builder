// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IOrderService
{
    OrderOptionsResponse GetOptions();
    Task<PagedResponse<OrderResponse>> GetAll(OrderQuery query);
    Task<OrderResponse?> GetById(int id);
    Task<OrderResponse?> Create(CreateOrderRequest request);
    Task<UpdateOrderResult> Update(int id, UpdateOrderRequest request);
    Task<bool> Delete(int id);
}
