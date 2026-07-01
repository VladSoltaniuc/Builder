// Application layer
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProductApi.Constants;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Exceptions;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class OrderService(AppDbContext db, IFileStorage files) : IOrderService
{
    private static readonly QueryShaper<Order> Shaper = new QueryShaper<Order>()
        .Search(o => o.User.Name)
        .Sort("status",                 o => o.Status)
        .Sort("totalPrice",             o => o.TotalPrice)
        .Sort("quantity",               o => o.Quantity)
        .Sort("createdAt",              o => o.CreatedAt);

    public OrderOptionsResponse GetOptions() => new(Enum.GetValues<OrderStatus>());

    public async Task<PagedResponse<OrderResponse>> GetAll(PageQuery q)
    {
        var query = db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .AsQueryable();

        query = Shaper.ApplySearch(query, q.Search);
        query = Shaper.ApplySort(query, q.SortBy);

        return await query.ToPagedResponse(q.Page, q.PageSize, o => ToResponse(o));
    }

    public async Task<OrderResponse> GetById(int id)
    {
        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);
        return ToResponse(order);
    }

    // Real systems get the AWB via courier's API. We generate it ourselves
    public async Task<OrderResponse> AssignGeneratedAwb(int id)
    {
        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        for (var attempt = 1; attempt <= OrderDefaults.AwbRetries; attempt++)
        {
            order.Awb = GenerateAwb();
            try
            {
                await db.SaveChangesAsync();
                return ToResponse(order);
            }
            catch (DbUpdateException ex)
                when (attempt < OrderDefaults.AwbRetries &&
                      ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
            {
                // Collided with an existing AWB loop and generate a fresh one
            }
        }

        throw new InvalidOperationException($"Could not generate a unique AWB after {OrderDefaults.AwbRetries} attempts.");
    }

    private static string GenerateAwb() =>
        $"SIM-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(0, 1_000_000):D6}";

    // Oversell safety  via to the place_order() stored function - lock+check+decrement
    public async Task<OrderResponse> Create(CreateOrderRequest request)
    {
        try
        {
            var newId = await db.Database
                .SqlQuery<int>(
                    $"""SELECT place_order({request.UserId}, {request.ProductId}, {request.Quantity}) AS "Value" """)
                .SingleAsync();

            return await GetById(newId);
        }
        catch (PostgresException ex) when (ex.MessageText is "USER_NOT_FOUND" or "PRODUCT_NOT_FOUND")
        {
            throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);
        }
        catch (PostgresException ex) when (ex.MessageText is "INSUFFICIENT_STOCK")
        {
            throw new UserFriendlyException("INSUFFICIENT_STOCK");
        }
    }

    public async Task<OrderResponse> Update(int id, UpdateOrderRequest request)
    {
        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        if (order.Version != request.Version)
            throw new UserFriendlyException("RESOURCE_CONFLICT", 409);

        order.Quantity = request.Quantity;
        order.TotalPrice = order.Product.Price * request.Quantity;
        order.Status = request.Status;
        order.Awb = request.Awb;
        order.Version++;

        await db.SaveChangesAsync();
        return ToResponse(order);
    }

    public async Task Delete(int id)
    {
        var order = await db.Orders.FindAsync(id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        var invoicePath = order.InvoicePath;
        db.Orders.Remove(order);
        await db.SaveChangesAsync();

        // Delete the file only after the row is gone, so a failed commit can't strand a row
        // pointing at a deleted file. Worst case now is a harmless orphaned file.
        files.DeleteIfPresent(invoicePath);
    }

    public async Task<OrderResponse> UploadInvoice(int id, IFormFile file)
    {
        var order = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        var previousPath = order.InvoicePath;
        order.InvoicePath = await files.Save(file, $"uploads/invoices/{id}.pdf");
        await db.SaveChangesAsync();

        files.DeleteReplaced(previousPath, order.InvoicePath);
        return ToResponse(order);
    }

    public async Task DeleteInvoice(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null || order.InvoicePath is null)
            throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        var invoicePath = order.InvoicePath;
        order.InvoicePath = null;
        await db.SaveChangesAsync();

        files.Delete(invoicePath);
    }

    public async Task<string> GetInvoicePath(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order?.InvoicePath is null)
            throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);
        return files.ResolvePath(order.InvoicePath);
    }

    private static OrderResponse ToResponse(Order o) =>
        new(o.Id, o.UserId, o.User.Name, o.ProductId, o.Product.Name, o.Quantity, o.TotalPrice, o.Status, o.CreatedAt, o.Version,
            o.InvoicePath is null ? null : $"/{o.InvoicePath}", o.Awb);
}
