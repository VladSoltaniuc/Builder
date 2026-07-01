// Application layer - the product bulk import/export feature, end to end
// Owns the ClosedXML dependency, the column mapping, AND its own persistence,
// so ProductService stays pure CRUD with no Excel traces
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Exceptions;
using ProductApi.Models;

namespace ProductApi.Services;

public sealed class ProductExcel(AppDbContext db)
{
    /// <summary>Loads all products and renders them to an .xlsx byte array, requested columns only.</summary>
    public async Task<byte[]> Export(IEnumerable<string> columns)
    {
        var products = await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        return Render(products, columns);
    }

    /// <summary>Parses, validates, and persists an uploaded .xlsx; returns the per-row outcome.</summary>
    public async Task<ImportProductResult> Import(IFormFile file)
    {
        var results = Parse(file);

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

    private static readonly (string Key, string Header, Func<Product, object?> Get)[] AllColumns =
    [
        ("id",       "ID",       p => (object)p.Id),
        ("name",     "Name",     p => p.Name),
        ("category", "Category", p => p.Category),
        ("price",    "Price",    p => (object)(double)p.Price),
        ("stock",    "Stock",    p => (object)p.Stock),
    ];

    private static byte[] Render(IReadOnlyList<Product> products, IEnumerable<string> columns)
    {
        var selected = columns.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var cols = AllColumns.Where(c => selected.Contains(c.Key)).ToArray();

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

    /// <summary>
    /// Parses and validates an uploaded .xlsx into per-row results: a Product on success,
    /// or an error string on failure. Throws MISSING_COLUMNS if a required header is absent
    /// Persisting the valid rows is the caller's job
    /// </summary>
    private static IReadOnlyList<(Product? Product, string? Error)> Parse(IFormFile file)
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

        // Read all cell values into plain structs on a single thread ClosedXML is not thread-safe
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

        // Validate and parse rows in parallel across all CPU cores
        // Each row is independent, so no synchronization is needed here
        return rawRows
            .AsParallel()
            .AsOrdered()
            .Select(r => ValidateRow(r.Row, r.Name, r.Category, r.PriceVal, r.PriceStr, r.StockVal, r.StockStr))
            .ToList();
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
            if (!Enum.TryParse<ProductCategory>(category, ignoreCase: true, out var parsedCategory))
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
}
