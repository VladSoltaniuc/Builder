// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Constants;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class OrderService(AppDbContext db, IWebHostEnvironment env) : IOrderService
{
    private static readonly Dictionary<string, System.Linq.Expressions.Expression<Func<Order, object>>> SortColumns = new()
    {
        ["status"]     = o => o.Status,
        ["totalPrice"] = o => o.TotalPrice,
        ["quantity"]   = o => o.Quantity,
        ["createdAt"]  = o => o.CreatedAt,
    };

    private static readonly Dictionary<string, Func<IQueryable<Order>, string, string, IQueryable<Order>>> FilterColumns = new()
    {
        ["status"] = (q, op, val) =>
        {
            if (!Enum.TryParse<OrderStatus>(val, ignoreCase: true, out var status)) return q;
            return op switch
            {
                "$eq"  => q.Where(o => o.Status == status),
                "$not" => q.Where(o => o.Status != status),
                _ => q
            };
        },
        ["totalPrice"] = (q, op, val) =>
        {
            if (op == "$btw") { var r = FilterHelper.ParseBtw(val); return r is null ? q : q.Where(o => o.TotalPrice >= r.Value.Min && o.TotalPrice <= r.Value.Max); }
            if (!decimal.TryParse(val, out var d)) return q;
            return op switch
            {
                "$eq"  => q.Where(o => o.TotalPrice == d),
                "$gt"  => q.Where(o => o.TotalPrice > d),
                "$gte" => q.Where(o => o.TotalPrice >= d),
                "$lt"  => q.Where(o => o.TotalPrice < d),
                "$lte" => q.Where(o => o.TotalPrice <= d),
                _ => q
            };
        },
        ["quantity"] = (q, op, val) =>
        {
            if (!int.TryParse(val, out var n)) return q;
            return op switch
            {
                "$eq"  => q.Where(o => o.Quantity == n),
                "$gt"  => q.Where(o => o.Quantity > n),
                "$gte" => q.Where(o => o.Quantity >= n),
                "$lt"  => q.Where(o => o.Quantity < n),
                "$lte" => q.Where(o => o.Quantity <= n),
                _ => q
            };
        },
    };

    public OrderOptionsResponse GetOptions() => new(OrderOptions.Statuses);

    public async Task<PagedResponse<OrderResponse>> GetAll(OrderQuery q)
    {
        var query = db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .AsQueryable();

        // --- Filter ---
        query = query.ApplyFilters(q.Filters, FilterColumns);

        // --- Search ---
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(o => EF.Functions.ILike(o.User.Name, $"%{q.Search}%") || EF.Functions.ILike(o.Product.Name, $"%{q.Search}%"));

        // --- Sort ---
        query = query.ApplySort(q.SortBy, SortColumns);

        var total = await query.CountAsync();
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(o => ToResponse(o))
            .ToListAsync();
        return new PagedResponse<OrderResponse>(items, total, q.Page, q.PageSize);
    }

    public async Task<OrderResponse?> GetById(int id)
    {
        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? null : ToResponse(order);
    }

    public async Task<OrderResponse?> Create(CreateOrderRequest request)
    {
        var user = await db.Users.FindAsync(request.UserId);
        var product = await db.Products.FindAsync(request.ProductId);

        if (user is null || product is null)
            return null;

        var order = new Order
        {
            UserId = request.UserId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            TotalPrice = product.Price * request.Quantity,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        order.User = user;
        order.Product = product;
        return ToResponse(order);
    }

    public async Task<UpdateOrderResult> Update(int id, UpdateOrderRequest request)
    {
        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order is null)
            return UpdateOrderResult.NotFound();

        if (order.Version != request.Version)
            return UpdateOrderResult.Conflict();

        order.Quantity = request.Quantity;
        order.TotalPrice = order.Product.Price * request.Quantity;
        order.Status = request.Status;
        order.Awb = request.Awb;
        order.Version++;

        await db.SaveChangesAsync();
        return UpdateOrderResult.Success(ToResponse(order));
    }

    public async Task<bool> Delete(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null)
            return false;

        if (order.InvoicePath is not null)
            DeleteFile(order.InvoicePath);

        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<OrderResponse?> UploadInvoice(int id, IFormFile file)
    {
        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return null;

        if (order.InvoicePath is not null)
            DeleteFile(order.InvoicePath);

        var relativePath = $"uploads/invoices/{id}.pdf";
        var fullPath = Path.Combine(env.WebRootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        order.InvoicePath = relativePath;
        await db.SaveChangesAsync();
        return ToResponse(order);
    }

    public async Task<bool> DeleteInvoice(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null || order.InvoicePath is null) return false;

        DeleteFile(order.InvoicePath);
        order.InvoicePath = null;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GetInvoicePath(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order?.InvoicePath is null) return null;
        return Path.Combine(env.WebRootPath, order.InvoicePath);
    }

    private void DeleteFile(string relativePath)
    {
        var fullPath = Path.Combine(env.WebRootPath, relativePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    private static OrderResponse ToResponse(Order o) =>
        new(o.Id, o.UserId, o.User.Name, o.ProductId, o.Product.Name, o.Quantity, o.TotalPrice, o.Status, o.CreatedAt, o.Version,
            o.InvoicePath is null ? null : $"/{o.InvoicePath}", o.Awb);
}
