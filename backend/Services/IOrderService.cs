// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IOrderService
{
    OrderOptionsResponse GetOptions();
    Task<PagedResponse<OrderResponse>> GetAll(PageQuery query);
    Task<OrderResponse?> GetById(int id);
    Task<OrderResponse?> AssignGeneratedAwb(int id);
    Task<OrderResponse?> Create(CreateOrderRequest request);
    Task<UpdateOrderResult> Update(int id, UpdateOrderRequest request);
    Task<bool> Delete(int id);
    Task<OrderResponse?> UploadInvoice(int id, IFormFile file);
    Task<bool> DeleteInvoice(int id);
    Task<string?> GetInvoicePath(int id);
}
