// Application layer
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ProductApi.Constants;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Exceptions;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class ProductService(AppDbContext db, IWebHostEnvironment env) : IProductService
{
    private static readonly EntityFilter<Product> Filter = new EntityFilter<Product>()
        .String("name",     p => p.Name)
        .Enum<ProductCategory>("category", p => p.Category)
        .Decimal("price",   p => p.Price)
        .Int("stock",       p => p.Stock)
        .Sort("name",       p => p.Name)
        .Sort("category",   p => (object)p.Category)
        .Sort("price",      p => p.Price)
        .Sort("stock",      p => p.Stock);

    public ProductOptionsResponse GetOptions() => new(Enum.GetValues<ProductCategory>());

    public async Task<PagedResponse<ProductResponse>> GetAll(PageQuery q)
    {
        var query = db.Products.AsQueryable();

        // --- Filter ---
        query = Filter.Apply(query, q.Filters);

        // --- Search ---
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{q.Search}%"));

        // --- Sort ---
        query = Filter.ApplySort(query, q.SortBy);

        return await query.ToPagedResponse(q.Page, q.PageSize, p => ToResponse(p));
    }

    // Substring search over Name, backed by the pg_trgm GIN index (ILIKE '%term%').
    public async Task<List<ProductResponse>> Search(string term)
    {
        var pattern = $"%{term}%";
        return await db.Products
            .AsNoTracking()
            .Where(p => EF.Functions.ILike(p.Name, pattern))
            .OrderBy(p => p.Id)
            .Take(50)
            .Select(p => ToResponse(p))
            .ToListAsync();
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

        // Read all cell values into plain structs on a single thread â€” ClosedXML is not thread-safe.
        var rawRows = Enumerable.Range(2, Math.Max(0, lastRow - 1))
            .Select(row => (
                Row:      row,
                Name:     CellString(ws.Cell(row, headers["name"])).Trim(),
                Category: CellString(ws.Cell(row, headers["category"])).Trim(),
                PriceVal: ws.Cell(row, headers["price"]).Value,
                PriceStr: CellString(ws.Cell(row, headers["price"])),
                StockVal: ws.Cell(row, headers["stock"]).Value,
                StockStr: CellString(ws.Cell(row, headers["stock"]))
            ))
            .ToList();

        // Validate and parse rows in parallel across all CPU cores.
        // Each row is independent, so no synchronization is needed here.
        var results = rawRows
            .AsParallel()
            .AsOrdered()
            .Select(r => ValidateRow(r.Row, r.Name, r.Category, r.PriceVal, r.PriceStr, r.StockVal, r.StockStr))
            .ToList();

        int imported = 0, failed = 0;
        var errors = new List<string>();

        foreach (var (product, error) in results)
        {
            if (product is not null) { db.Products.Add(product); imported++; }
            else                     { errors.Add(error!);       failed++;   }
        }

        if (imported > 0)
            await db.SaveChangesAsync();

        return new ImportProductResult(imported, failed, errors);
    }

    private static (Product? Product, string? Error) ValidateRow(
        int row, string name, string category,
        XLCellValue priceVal, string priceStr,
        XLCellValue stockVal, string stockStr)
    {
        try
        {
            if (name.Length < 2)
                return (null, $"Row {row}: Name must be at least 2 characters");
            if (!System.Enum.TryParse<ProductCategory>(category, ignoreCase: true, out var parsedCategory))
                return (null, $"Row {row}: Invalid category '{category}'");

            decimal price = priceVal.IsNumber
                ? (decimal)priceVal.GetNumber()
                : decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var pd) ? pd : -1;
            if (price <= 0)
                return (null, $"Row {row}: Price must be greater than 0");

            int stock = stockVal.IsNumber
                ? (int)Math.Round(stockVal.GetNumber())
                : int.TryParse(stockStr, out var si) ? si : -1;
            if (stock < 0)
                return (null, $"Row {row}: Stock must be a non-negative integer");

            return (new Product { Name = name, Category = parsedCategory, Price = price, Stock = stock }, null);
        }
        catch (Exception ex)
        {
            return (null, $"Row {row}: {ex.Message}");
        }
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
        new(p.Id, p.Name, p.Category.ToString(), p.Price, p.Stock, p.Version,
            p.ImagePath is null ? null : $"/{p.ImagePath}");
}
