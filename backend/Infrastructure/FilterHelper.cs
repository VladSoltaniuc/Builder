// Infrastructure layer
namespace ProductApi.Infrastructure;

public static class FilterHelper
{
    public static IQueryable<T> ApplyFilters<T>(
        this IQueryable<T> query,
        Dictionary<string, string> filters,
        Dictionary<string, Func<IQueryable<T>, string, string, IQueryable<T>>> columns)
    {
        foreach (var (key, raw) in filters)
        {
            if (!columns.TryGetValue(key, out var apply))
                continue;

            var (op, val) = ParseFilter(raw);
            query = apply(query, op, val);
        }
        return query;
    }

    public static (string Op, string Val) ParseFilter(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || !raw.StartsWith('$'))
            return ("$eq", raw?.Trim() ?? string.Empty);

        var colon = raw.IndexOf(':');
        return colon < 0
            ? (raw, string.Empty)
            : (raw[..colon], raw[(colon + 1)..]);
    }

    // Parses "$btw:100,500" value part into two decimals
    public static (decimal Min, decimal Max)? ParseBtw(string val)
    {
        var parts = val.Split(',');
        if (parts.Length != 2) return null;
        if (!decimal.TryParse(parts[0], out var min)) return null;
        if (!decimal.TryParse(parts[1], out var max)) return null;
        return (min, max);
    }

    // Parses "$in:1,2,3" value part into a list of ints
    public static List<int>? ParseInInt(string val)
    {
        var results = new List<int>();
        foreach (var part in val.Split(','))
        {
            if (!int.TryParse(part.Trim(), out var n)) return null;
            results.Add(n);
        }
        return results;
    }
}
