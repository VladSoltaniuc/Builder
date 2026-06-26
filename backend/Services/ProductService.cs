// Application layer
using System.Globalization;
using ClosedXML.Excel;
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

    public async Task<byte[]> ExportToExcel(IEnumerable<string> columns)
    {
        var selected = columns.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var products = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();

        var allCols = new (string Key, string Header, Func<Product, object?> Get)[]
        {
            ("id",       "ID",       p => (object)p.Id),
            ("name",     "Name",     p => p.Name),
            ("category", "Category", p => p.Category),
            ("price",    "Price",    p => (object)(double)p.Price),
            ("stock",    "Stock",    p => (object)p.Stock),
        };

        var cols = allCols.Where(c => selected.Contains(c.Key)).ToArray();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Products");

        for (int i = 0; i < cols.Length; i++)
            ws.Cell(1, i + 1).Value = cols[i].Header;

        for (int r = 0; r < products.Count; r++)
        {
            var p = products[r];
            for (int c = 0; c < cols.Length; c++)
            {
                var cell = ws.Cell(r + 2, c + 1);
                switch (cols[c].Get(p))
                {
                    case int n:    cell.Value = n; break;
                    case double d: cell.Value = d; break;
                    case string s: cell.Value = s; break;
                }
            }
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<ImportProductResult> ImportFromExcel(IFormFile file)
    {
        var errors = new List<string>();
        int imported = 0, failed = 0;

        using var stream = file.OpenReadStream();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();

        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in ws.Row(1).CellsUsed())
            headers[cell.Value.ToString()] = cell.Address.ColumnNumber;

        var required = new[] { "name", "category", "price", "stock" };
        var missing = required.Where(r => !headers.ContainsKey(r)).ToArray();
        if (missing.Length > 0)
            throw new UserFriendlyException(
                $"Missing required columns: {string.Join(", ", missing)}",
                "MISSING_COLUMNS",
                string.Join(", ", missing));

        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        for (int row = 2; row <= lastRow; row++)
        {
            try
            {
                var name = CellString(ws.Cell(row, headers["name"])).Trim();
                var category = CellString(ws.Cell(row, headers["category"])).Trim();

                if (name.Length < 2)
                { errors.Add($"Row {row}: Name must be at least 2 characters"); failed++; continue; }
                if (string.IsNullOrEmpty(category))
                { errors.Add($"Row {row}: Category is required"); failed++; continue; }

                var priceVal = ws.Cell(row, headers["price"]).Value;
                decimal price = priceVal.IsNumber
                    ? (decimal)priceVal.GetNumber()
                    : decimal.TryParse(CellString(ws.Cell(row, headers["price"])), NumberStyles.Any, CultureInfo.InvariantCulture, out var pd) ? pd : -1;
                if (price <= 0)
                { errors.Add($"Row {row}: Price must be greater than 0"); failed++; continue; }

                var stockVal = ws.Cell(row, headers["stock"]).Value;
                int stock = stockVal.IsNumber
                    ? (int)Math.Round(stockVal.GetNumber())
                    : int.TryParse(CellString(ws.Cell(row, headers["stock"])), out var si) ? si : -1;
                if (stock < 0)
                { errors.Add($"Row {row}: Stock must be a non-negative integer"); failed++; continue; }

                db.Products.Add(new Product { Name = name, Category = category, Price = price, Stock = stock });
                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {row}: {ex.Message}");
                failed++;
            }
        }

        if (imported > 0)
            await db.SaveChangesAsync();

        return new ImportProductResult(imported, failed, errors);
    }

    private static string CellString(IXLCell cell)
    {
        var v = cell.Value;
        if (v.IsBlank) return string.Empty;
        if (v.IsText) return v.GetText();
        if (v.IsNumber) return v.GetNumber().ToString(CultureInfo.InvariantCulture);
        return v.ToString();
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
