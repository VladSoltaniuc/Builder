// Infrastructure layer — pagination utility
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;

namespace ProductApi.Infrastructure;

public static class PaginationHelper
{
    // Counts the full result set, then pulls one page and projects each row to a response.
    // Count and the page query run as two round-trips against the same filtered IQueryable.
    public static async Task<PagedResponse<TResponse>> ToPagedResponse<TEntity, TResponse>(
        this IQueryable<TEntity> query,
        int page,
        int pageSize,
        Expression<Func<TEntity, TResponse>> selector)
    {
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync();
        return new PagedResponse<TResponse>(items, total, page, pageSize);
    }
}
