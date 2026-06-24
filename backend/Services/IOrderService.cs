// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IOrderService
{
    Task<PagedResponse<OrderResponse>> GetAll(int page, int pageSize);
    Task<OrderResponse?> GetById(int id);
    Task<OrderResponse?> Create(CreateOrderRequest request);
    Task<UpdateOrderResult> Update(int id, UpdateOrderRequest request);
    Task<bool> Delete(int id);
}
