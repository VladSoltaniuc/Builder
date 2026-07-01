// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Exceptions;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class ProductService(AppDbContext db, IFileStorage files) : IProductService
{
    private static readonly QueryShaper<Product> Shaper = new QueryShaper<Product>()
        .Sort("category",   p => (object)p.Category)
        .Sort("price",      p => p.Price)
        .Sort("stock",      p => p.Stock)
        .Sort("name",       p => p.Name)
        .Search(p => p.Name);

    public ProductOptionsResponse GetOptions() => new(Enum.GetValues<ProductCategory>());

    public async Task<PagedResponse<ProductResponse>> GetAll(PageQuery q)
    {
        var query = db.Products.AsQueryable();
        query = Shaper.ApplySearch(query, q.Search);
        query = Shaper.ApplySort(query, q.SortBy);
        return await query.ToPagedResponse(q.Page, q.PageSize, p => ToResponse(p));
    }

    public async Task<ProductResponse> GetById(int id)
    {
        var product = await db.Products.FindAsync(id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);
        return ToResponse(product);
    }

    public async Task<ProductResponse> Create(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Category = request.Category,
            Price = request.Price,
            Stock = request.Stock,
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return ToResponse(product);
    }

    public async Task<ProductResponse> Update(int id, UpdateProductRequest request)
    {
        var product = await db.Products.FindAsync(id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        if (product.Version != request.Version)
            throw new UserFriendlyException("RESOURCE_CONFLICT", 409);

        product.Name = request.Name;
        product.Category = request.Category;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Version++;

        await db.SaveChangesAsync();
        return ToResponse(product);
    }

    public async Task Delete(int id)
    {
        var product = await db.Products.FindAsync(id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        var imagePath = product.ImagePath;
        db.Products.Remove(product);
        await db.SaveChangesAsync();

        files.DeleteIfPresent(imagePath);
    }

    public async Task<ProductResponse> UploadImage(int id, IFormFile file)
    {
        var product = await db.Products.FindAsync(id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        var previousPath = product.ImagePath;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        product.ImagePath = await files.Save(file, $"uploads/products/{id}{ext}");
        await db.SaveChangesAsync();

        files.DeleteReplaced(previousPath, product.ImagePath);
        return ToResponse(product);
    }

    public async Task DeleteImage(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null || product.ImagePath is null)
            throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        var imagePath = product.ImagePath;
        product.ImagePath = null;
        await db.SaveChangesAsync();

        files.Delete(imagePath);
    }

    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.Category.ToString(), p.Price, p.Stock, p.Version,
            p.ImagePath is null ? null : $"/{p.ImagePath}");
}
