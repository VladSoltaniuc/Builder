using ProductApi.Contracts;

namespace ProductApi.Services;

/// <summary>
/// Interfața (contractul) serviciului de produse.
/// Controller-ul depinde DOAR de această interfață, nu de implementarea concretă.
/// Avantaje: poți schimba implementarea (ex: din memorie în bază de date)
/// fără să atingi controller-ul, și poți scrie teste cu un mock.
/// </summary>
public interface IProductService
{
    PagedResponse<ProductResponse> GetAll(int page, int pageSize);

    ProductResponse? GetById(int id);

    ProductResponse Create(CreateProductRequest request);

    /// <returns>Success with updated product, NotFound, or Conflict if version mismatches.</returns>
    UpdateProductResult Update(int id, UpdateProductRequest request);

    /// <returns>true dacă a fost șters, false dacă nu exista.</returns>
    bool Delete(int id);
}
