// Application layer
using ProductApi.Constants;

namespace ProductApi.Contracts;

public class PageQuery
{
    public int Page { get; set; } = PaginationDefaults.Page;

    private int _pageSize = PaginationDefaults.PageSizes[0];
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = PaginationDefaults.PageSizes.Contains(value)
            ? value
            : PaginationDefaults.PageSizes[0];
    }
    public string? SortBy { get; set; }
    public string? Search { get; set; }
}
