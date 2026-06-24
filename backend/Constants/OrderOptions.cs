// Shared layer — order domain constants
using ProductApi.Models;

namespace ProductApi.Constants;
public static class OrderOptions
{
    public static readonly string[] Statuses = Enum.GetNames<OrderStatus>();
}
