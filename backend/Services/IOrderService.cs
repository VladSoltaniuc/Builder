// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IOrderService
{
    OrderOptionsResponse GetOptions();
    Task<PagedResponse<OrderResponse>> GetAll(PageQuery query);
    Task<OrderResponse> GetById(int id);
    Task<OrderResponse> AssignGeneratedAwb(int id);
    Task<OrderResponse> Create(CreateOrderRequest request);
    Task<OrderResponse> Update(int id, UpdateOrderRequest request);
    Task Delete(int id);
    Task<OrderResponse> UploadInvoice(int id, IFormFile file);
    Task DeleteInvoice(int id);
    Task<string> GetInvoicePath(int id);
}
