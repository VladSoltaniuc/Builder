// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IProductService
{
    ProductOptionsResponse GetOptions();
    Task<PagedResponse<ProductResponse>> GetAll(ProductQuery query);
    Task<List<ProductResponse>> Search(string term);
    Task<ProductResponse?> GetById(int id);
    Task<ProductResponse> Create(CreateProductRequest request);
    Task<UpdateProductResult> Update(int id, UpdateProductRequest request);
    Task<bool> Delete(int id);
    Task<ProductResponse?> UploadImage(int id, IFormFile file);
    Task<bool> DeleteImage(int id);
    Task<byte[]> ExportToExcel(IEnumerable<string> columns);
    Task<ImportProductResult> ImportFromExcel(IFormFile file);
}
