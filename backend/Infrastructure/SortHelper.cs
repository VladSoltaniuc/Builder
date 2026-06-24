// Infrastructure layer — sorting utility
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ProductApi.Infrastructure;

public static class SortHelper
{
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortBy,
        Dictionary<string, Expression<Func<T, object>>> columns)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var parts = sortBy.Split(':');
        var field = parts[0].ToLower();
        var dir = parts.Length > 1 ? parts[1].ToUpper() : "ASC";

        if (!columns.TryGetValue(field, out var selector))
            return query;

        return dir == "DESC"
            ? query.OrderByDescending(selector)
            : query.OrderBy(selector);
    }
}
