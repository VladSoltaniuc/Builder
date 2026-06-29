// Infrastructure layer
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;

namespace ProductApi.Infrastructure;

public static class PaginationHelper
{
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
