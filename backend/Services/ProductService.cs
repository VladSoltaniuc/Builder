// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Constants;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class ProductService(AppDbContext db, IWebHostEnvironment env) : IProductService
{
    private static readonly Dictionary<string, System.Linq.Expressions.Expression<Func<Product, object>>> SortColumns = new()
    {
        ["name"]     = p => p.Name,
        ["category"] = p => p.Category,
        ["price"]    = p => p.Price,
        ["stock"]    = p => p.Stock,
    };

    // String columns: all ops are case-insensitive (ILike)
    // Numeric columns: support $eq $gt $gte $lt $lte $btw $in
    private static readonly Dictionary<string, Func<IQueryable<Product>, string, string, IQueryable<Product>>> FilterColumns = new()
    {
        ["name"] = (q, op, val) => op switch
        {
            "$eq"    => q.Where(p => EF.Functions.ILike(p.Name, val)),
            "$not"   => q.Where(p => !EF.Functions.ILike(p.Name, val)),
            "$ilike" => q.Where(p => EF.Functions.ILike(p.Name, $"%{val}%")),
            "$sw"    => q.Where(p => EF.Functions.ILike(p.Name, $"{val}%")),
            _ => q
        },
        ["category"] = (q, op, val) => op switch
        {
            "$eq"    => q.Where(p => EF.Functions.ILike(p.Category, val)),
            "$not"   => q.Where(p => !EF.Functions.ILike(p.Category, val)),
            "$ilike" => q.Where(p => EF.Functions.ILike(p.Category, $"%{val}%")),
            "$sw"    => q.Where(p => EF.Functions.ILike(p.Category, $"{val}%")),
            _ => q
        },
        ["price"] = (q, op, val) =>
        {
            if (op == "$btw") { var r = FilterHelper.ParseBtw(val); return r is null ? q : q.Where(p => p.Price >= r.Value.Min && p.Price <= r.Value.Max); }
            if (!decimal.TryParse(val, out var d)) return q;
            return op switch
            {
                "$eq"  => q.Where(p => p.Price == d),
                "$gt"  => q.Where(p => p.Price > d),
                "$gte" => q.Where(p => p.Price >= d),
                "$lt"  => q.Where(p => p.Price < d),
                "$lte" => q.Where(p => p.Price <= d),
                _ => q
            };
        },
        ["stock"] = (q, op, val) =>
        {
            if (op == "$in") { var ids = FilterHelper.ParseInInt(val); return ids is null ? q : q.Where(p => ids.Contains(p.Stock)); }
            if (!int.TryParse(val, out var n)) return q;
            return op switch
            {
                "$eq"  => q.Where(p => p.Stock == n),
                "$gt"  => q.Where(p => p.Stock > n),
                "$gte" => q.Where(p => p.Stock >= n),
                "$lt"  => q.Where(p => p.Stock < n),
                "$lte" => q.Where(p => p.Stock <= n),
                _ => q
            };
        },
    };

    public ProductOptionsResponse GetOptions() => new(ProductOptions.Categories);

    public async Task<PagedResponse<ProductResponse>> GetAll(ProductQuery q)
    {
        var query = db.Products.AsQueryable();

        // --- Filter ---
        query = query.ApplyFilters(q.Filters, FilterColumns);

        // --- Search ---
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{q.Search}%") || EF.Functions.ILike(p.Category, $"%{q.Search}%"));

        // --- Sort ---
        query = query.ApplySort(q.SortBy, SortColumns);

        var total = await query.CountAsync();
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(p => ToResponse(p))
            .ToListAsync();
        return new PagedResponse<ProductResponse>(items, total, q.Page, q.PageSize);
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

        if (product.ImagePath is not null)
            DeleteFile(product.ImagePath);

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ProductResponse?> UploadImage(int id, IFormFile file)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return null;

        if (product.ImagePath is not null)
            DeleteFile(product.ImagePath);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var relativePath = $"uploads/products/{id}{ext}";
        var fullPath = Path.Combine(env.WebRootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        product.ImagePath = relativePath;
        await db.SaveChangesAsync();
        return ToResponse(product);
    }

    public async Task<bool> DeleteImage(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null || product.ImagePath is null) return false;

        DeleteFile(product.ImagePath);
        product.ImagePath = null;
        await db.SaveChangesAsync();
        return true;
    }

    private void DeleteFile(string relativePath)
    {
        var fullPath = Path.Combine(env.WebRootPath, relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    private static ProductResponse ToResponse(Product p) =>
        new(p.Id, p.Name, p.Category, p.Price, p.Stock, p.Version,
            p.ImagePath is null ? null : $"/{p.ImagePath}");
}
