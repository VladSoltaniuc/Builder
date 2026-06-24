// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services;

public class OrderService(AppDbContext db) : IOrderService
{
    public async Task<PagedResponse<OrderResponse>> GetAll(int page, int pageSize)
    {
        var total = await db.Orders.CountAsync();
        var items = await db.Orders
            .Include(o => o.User)
            .Include(o => o.Product)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => ToResponse(o))
            .ToListAsync();
        return new PagedResponse<OrderResponse>(items, total, page, pageSize);
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
            Status = "Pending",
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
        order.Version++;

        await db.SaveChangesAsync();
        return UpdateOrderResult.Success(ToResponse(order));
    }

    public async Task<bool> Delete(int id)
    {
        var order = await db.Orders.FindAsync(id);
        if (order is null)
            return false;

        db.Orders.Remove(order);
        await db.SaveChangesAsync();
        return true;
    }

    private static OrderResponse ToResponse(Order o) =>
        new(o.Id, o.UserId, o.User.Name, o.ProductId, o.Product.Name, o.Quantity, o.TotalPrice, o.Status, o.CreatedAt, o.Version);
}
