// Infrastructure layer — generic filter/sort builder for EF Core + PostgreSQL
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ProductApi.Infrastructure;

public sealed class EntityFilter<T>
{
    // Capture the ILike method and EF.Functions argument from a seed lambda — avoids raw string-based reflection.
    private static readonly MethodInfo ILikeMethod;
    private static readonly Expression EfFunctionsExpr;

    static EntityFilter()
    {
        Expression<Func<bool>> seed = () => EF.Functions.ILike("", "");
        var call = (MethodCallExpression)seed.Body;
        ILikeMethod = call.Method;
        EfFunctionsExpr = call.Arguments[0];
    }

    private readonly Dictionary<string, Func<IQueryable<T>, string, string, IQueryable<T>>> _filters = new();
    private readonly Dictionary<string, Expression<Func<T, object>>> _sorts = new();

    // String columns: $eq, $not, $ilike (contains), $sw (starts-with)
    public EntityFilter<T> String(string column, Expression<Func<T, string>> prop)
    {
        _filters[column] = (q, op, val) => op switch
        {
            "$not"   => q.Where(BuildNot(BuildILike(prop, val))),
            "$ilike" => q.Where(BuildILike(prop, $"%{val}%")),
            "$sw"    => q.Where(BuildILike(prop, $"{val}%")),
            _        => q.Where(BuildILike(prop, val)),
        };
        return this;
    }

    // Integer columns: $eq, $not, $gt, $gte, $lt, $lte, $in
    public EntityFilter<T> Int(string column, Expression<Func<T, int>> prop)
    {
        _filters[column] = (q, op, val) =>
        {
            if (op == "$in") { var ids = FilterHelper.ParseInInt(val); return ids is null ? q : q.Where(BuildIn(prop, ids)); }
            if (!int.TryParse(val, out var n)) return q;
            return op switch
            {
                "$not" => q.Where(BuildCmp(prop, n, ExpressionType.NotEqual)),
                "$gt"  => q.Where(BuildCmp(prop, n, ExpressionType.GreaterThan)),
                "$gte" => q.Where(BuildCmp(prop, n, ExpressionType.GreaterThanOrEqual)),
                "$lt"  => q.Where(BuildCmp(prop, n, ExpressionType.LessThan)),
                "$lte" => q.Where(BuildCmp(prop, n, ExpressionType.LessThanOrEqual)),
                _      => q.Where(BuildCmp(prop, n, ExpressionType.Equal)),
            };
        };
        return this;
    }

    // Decimal columns: $eq, $not, $gt, $gte, $lt, $lte, $btw
    public EntityFilter<T> Decimal(string column, Expression<Func<T, decimal>> prop)
    {
        _filters[column] = (q, op, val) =>
        {
            if (op == "$btw") { var r = FilterHelper.ParseBtw(val); return r is null ? q : q.Where(BuildBtw(prop, r.Value.Min, r.Value.Max)); }
            if (!decimal.TryParse(val, out var d)) return q;
            return op switch
            {
                "$not" => q.Where(BuildCmp(prop, d, ExpressionType.NotEqual)),
                "$gt"  => q.Where(BuildCmp(prop, d, ExpressionType.GreaterThan)),
                "$gte" => q.Where(BuildCmp(prop, d, ExpressionType.GreaterThanOrEqual)),
                "$lt"  => q.Where(BuildCmp(prop, d, ExpressionType.LessThan)),
                "$lte" => q.Where(BuildCmp(prop, d, ExpressionType.LessThanOrEqual)),
                _      => q.Where(BuildCmp(prop, d, ExpressionType.Equal)),
            };
        };
        return this;
    }

    // Enum columns: $eq, $not
    public EntityFilter<T> Enum<TEnum>(string column, Expression<Func<T, TEnum>> prop)
        where TEnum : struct, System.Enum
    {
        _filters[column] = (q, op, val) =>
        {
            if (!System.Enum.TryParse<TEnum>(val, ignoreCase: true, out var e)) return q;
            return op == "$not"
                ? q.Where(BuildCmp(prop, e, ExpressionType.NotEqual))
                : q.Where(BuildCmp(prop, e, ExpressionType.Equal));
        };
        return this;
    }

    public EntityFilter<T> Sort(string column, Expression<Func<T, object>> selector)
    {
        _sorts[column] = selector;
        return this;
    }

    public IQueryable<T> Apply(IQueryable<T> query, Dictionary<string, string> filters)
        => query.ApplyFilters(filters, _filters);

    public IQueryable<T> ApplySort(IQueryable<T> query, string? sortBy)
        => query.ApplySort(sortBy, _sorts);

    // --- Expression builders ---

    private static Expression<Func<T, bool>> BuildILike(Expression<Func<T, string>> prop, string pattern)
    {
        var call = Expression.Call(ILikeMethod, EfFunctionsExpr, prop.Body, Expression.Constant(pattern));
        return Expression.Lambda<Func<T, bool>>(call, prop.Parameters[0]);
    }

    private static Expression<Func<T, bool>> BuildNot(Expression<Func<T, bool>> expr)
        => Expression.Lambda<Func<T, bool>>(Expression.Not(expr.Body), expr.Parameters[0]);

    private static Expression<Func<T, bool>> BuildCmp<TVal>(
        Expression<Func<T, TVal>> prop, TVal val, ExpressionType op)
    {
        var rhs = Expression.Constant(val, typeof(TVal));
        var body = op switch
        {
            ExpressionType.Equal              => Expression.Equal(prop.Body, rhs),
            ExpressionType.NotEqual           => Expression.NotEqual(prop.Body, rhs),
            ExpressionType.GreaterThan        => Expression.GreaterThan(prop.Body, rhs),
            ExpressionType.GreaterThanOrEqual => Expression.GreaterThanOrEqual(prop.Body, rhs),
            ExpressionType.LessThan           => Expression.LessThan(prop.Body, rhs),
            ExpressionType.LessThanOrEqual    => Expression.LessThanOrEqual(prop.Body, rhs),
            _ => throw new ArgumentOutOfRangeException(nameof(op))
        };
        return Expression.Lambda<Func<T, bool>>(body, prop.Parameters[0]);
    }

    private static Expression<Func<T, bool>> BuildIn(Expression<Func<T, int>> prop, List<int> ids)
    {
        var contains = typeof(List<int>).GetMethod(nameof(List<int>.Contains))!;
        var call = Expression.Call(Expression.Constant(ids), contains, prop.Body);
        return Expression.Lambda<Func<T, bool>>(call, prop.Parameters[0]);
    }

    private static Expression<Func<T, bool>> BuildBtw(Expression<Func<T, decimal>> prop, decimal min, decimal max)
    {
        var gte = Expression.GreaterThanOrEqual(prop.Body, Expression.Constant(min));
        var lte = Expression.LessThanOrEqual(prop.Body, Expression.Constant(max));
        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(gte, lte), prop.Parameters[0]);
    }
}
