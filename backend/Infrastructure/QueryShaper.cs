// Infrastructure layer
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ProductApi.Infrastructure;

public sealed class QueryShaper<T>
{
    private static readonly MethodInfo ILikeMethod;
    private static readonly Expression EfFunctionsExpr;

    static QueryShaper()
    {
        Expression<Func<bool>> seed = () => EF.Functions.ILike("", "");
        var call = (MethodCallExpression)seed.Body;
        ILikeMethod = call.Method;
        EfFunctionsExpr = call.Arguments[0];
    }

    private readonly Dictionary<string, Expression<Func<T, object>>> _sorts = new();
    private readonly List<Expression<Func<T, string>>> _searches = new();

    /// <summary>Register one or more text columns the free-text search box matches against (contains, OR-ed).</summary>
    public QueryShaper<T> Search(params Expression<Func<T, string>>[] props)
    {
        _searches.AddRange(props);
        return this;
    }

    /// <summary>Allow ordering results by this column (e.g. ?sortBy=name, or -name for descending).</summary>
    public QueryShaper<T> Sort(string column, Expression<Func<T, object>> selector)
    {
        _sorts[column] = selector;
        return this;
    }

    /// <summary>Apply the free-text search term across all registered search columns (skips blank terms).</summary>
    public IQueryable<T> ApplySearch(IQueryable<T> query, string? term)
    {
        if (string.IsNullOrWhiteSpace(term) || _searches.Count == 0)
            return query;

        var pattern = $"%{term}%";
        Expression<Func<T, bool>>? combined = null;
        foreach (var prop in _searches)
        {
            var like = BuildILike(prop, pattern);
            combined = combined is null ? like : OrElse(combined, like);
        }
        return combined is null ? query : query.Where(combined);
    }

    /// <summary>Apply the registered sort to the query based on the request's sortBy value.</summary>
    public IQueryable<T> ApplySort(IQueryable<T> query, string? sortBy)
        => query.ApplySort(sortBy, _sorts);

    // --- Expression builders ---

    private static Expression<Func<T, bool>> BuildILike(Expression<Func<T, string>> prop, string pattern)
    {
        var call = Expression.Call(ILikeMethod, EfFunctionsExpr, prop.Body, Expression.Constant(pattern));
        return Expression.Lambda<Func<T, bool>>(call, prop.Parameters[0]);
    }

    // ORs two predicates, rebinding the second's parameter so both share one (required for a valid tree)
    private static Expression<Func<T, bool>> OrElse(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
    {
        var param = a.Parameters[0];
        var bBody = new ReplaceParam(b.Parameters[0], param).Visit(b.Body);
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(a.Body, bBody), param);
    }

    private sealed class ReplaceParam(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => node == from ? to : base.VisitParameter(node);
    }
}
