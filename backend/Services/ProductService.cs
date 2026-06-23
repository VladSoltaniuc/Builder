// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services;

public class ProductService(AppDbContext db) : IProductService
{
    public async Task<PagedResponse<ProductResponse>> GetAll(int page, int pageSize)
    {
        var total = await db.Products.CountAsync();
        var items = await db.Products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => ToResponse(p))
            .ToListAsync();
        return new PagedResponse<ProductResponse>(items, total, page, pageSize);
    }

    public async Task<ProductResponse?> GetById(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? null : ToResponse(product);
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

    public async Task<UpdateProductResult> Update(int id, UpdateProductRequest request)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
            return UpdateProductResult.NotFound();

        if (product.Version != request.Version)
            return UpdateProductResult.Conflict();

        product.Name = request.Name;
        product.Category = request.Category;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Version++;

        await db.SaveChangesAsync();
        return UpdateProductResult.Success(ToResponse(product));
    }

    public async Task<bool> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null)
            return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return true;
    }

    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.Category, p.Price, p.Stock, p.Version);
}
