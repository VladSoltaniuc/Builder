// Application layer — base query object
using Microsoft.AspNetCore.Mvc;
using ProductApi.Constants;

namespace ProductApi.Contracts;

public class PageQuery
{
    public int Page { get; set; } = PaginationDefaults.Page;
    public int PageSize { get; set; } = PaginationDefaults.PageSize;
    public string? SortBy { get; set; }
    public string? Search { get; set; }

    // Binds filter.field=$op:value from query string
    [FromQuery(Name = "filter")]
    public Dictionary<string, string> Filters { get; set; } = new();
}
