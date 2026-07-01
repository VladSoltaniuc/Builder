// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IProductService
{
    ProductOptionsResponse GetOptions();
    Task<PagedResponse<ProductResponse>> GetAll(PageQuery query);
    Task<ProductResponse> GetById(int id);
    Task<ProductResponse> Create(CreateProductRequest request);
    Task<ProductResponse> Update(int id, UpdateProductRequest request);
    Task Delete(int id);
    Task<ProductResponse> UploadImage(int id, IFormFile file);
    Task DeleteImage(int id);
}
