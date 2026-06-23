// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetAll(int page, int pageSize);
    Task<ProductResponse?> GetById(int id);
    Task<ProductResponse> Create(CreateProductRequest request);
    Task<UpdateProductResult> Update(int id, UpdateProductRequest request);
    Task<bool> Delete(int id);
}
