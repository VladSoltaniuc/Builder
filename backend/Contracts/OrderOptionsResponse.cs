// Application layer
using ProductApi.Models;

namespace ProductApi.Contracts;

public record OrderOptionsResponse(OrderStatus[] Statuses);
