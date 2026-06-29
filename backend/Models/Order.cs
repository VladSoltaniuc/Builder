// Domain layer
namespace ProductApi.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;

    public string? InvoicePath { get; set; }
    public string? Awb { get; set; }
}

public enum OrderStatus
{
    Pending   = 0,
    Completed = 1,
    Cancelled = 2,
}
