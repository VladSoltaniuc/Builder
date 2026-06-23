// Application layer
namespace ProductApi.Contracts;

public record PagedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
