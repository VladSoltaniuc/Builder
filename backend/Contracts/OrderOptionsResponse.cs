// Application layer
using ProductApi.Constants;

namespace ProductApi.Contracts;

public record OrderOptionsResponse(OrderStatus[] Statuses);
