using ProductApi.Contracts;
using ProductApi.Models;

namespace ProductApi.Services;

/// <summary>
/// Implementarea serviciului. Datele sunt ținute în memorie (hardcodate la pornire).
/// Într-un proiect real, aici ai injecta un DbContext (Entity Framework) și ai vorbi cu baza de date,
/// dar restul aplicației (controller, frontend) ar rămâne neschimbat - de asta folosim interfața.
/// </summary>
public class ProductService : IProductService
{
    // Lista internă de entități. 'readonly' = referința nu se schimbă, dar conținutul da.
    private readonly List<Product> _products;

    // Serviciul e Singleton, deci poate fi accesat de pe mai multe request-uri în paralel.
    // 'lock' protejează lista ca să nu apară coruperi de date (race conditions).
    private readonly object _lock = new();

    // Următorul Id de atribuit. Echivalentul unui auto-increment din baza de date.
    private int _nextId;

    public ProductService()
    {
        // ---- Date hardcodate (seed) ----
        _products = new List<Product>
        {
            new() { Id = 1, Name = "Tastatură mecanică", Category = "Periferice", Price = 349.99m, Stock = 25 },
            new() { Id = 2, Name = "Mouse wireless",     Category = "Periferice", Price = 149.50m, Stock = 60 },
            new() { Id = 3, Name = "Monitor 27\" 144Hz",  Category = "Monitoare",  Price = 1299.00m, Stock = 12 },
            new() { Id = 4, Name = "SSD NVMe 1TB",        Category = "Stocare",    Price = 459.00m, Stock = 40 },
            new() { Id = 5, Name = "Căști gaming",        Category = "Audio",      Price = 279.99m, Stock = 18 },
        };

        // Pornim id-urile noi de la maximul existent + 1.
        _nextId = _products.Max(p => p.Id) + 1;
    }

    public PagedResponse<ProductResponse> GetAll(int page, int pageSize)
    {
        lock (_lock)
        {
            var total = _products.Count;
            var items = _products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ToResponse)
                .ToList();
            return new PagedResponse<ProductResponse>(items, total, page, pageSize);
        }
    }

    public ProductResponse? GetById(int id)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            return product is null ? null : ToResponse(product);
        }
    }

    public ProductResponse Create(CreateProductRequest request)
    {
        lock (_lock)
        {
            var product = new Product
            {
                Id = _nextId++,
                Name = request.Name,
                Category = request.Category,
                Price = request.Price,
                Stock = request.Stock,
            };

            _products.Add(product);
            return ToResponse(product);
        }
    }

    public UpdateProductResult Update(int id, UpdateProductRequest request)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product is null)
                return UpdateProductResult.NotFound();

            if (product.Version != request.Version)
                return UpdateProductResult.Conflict();

            product.Name = request.Name;
            product.Category = request.Category;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.Version++;

            return UpdateProductResult.Success(ToResponse(product));
        }
    }

    public bool Delete(int id)
    {
        lock (_lock)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product is null)
            {
                return false;
            }

            _products.Remove(product);
            return true;
        }
    }

    // Funcție de mapare entitate -> DTO, ținută într-un singur loc (DRY).
    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.Category, p.Price, p.Stock, p.Version);
}
