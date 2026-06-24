// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class UserService(AppDbContext db) : IUserService
{
    private static readonly Dictionary<string, System.Linq.Expressions.Expression<Func<User, object>>> SortColumns = new()
    {
        ["name"]  = u => u.Name,
        ["email"] = u => u.Email,
    };

    private static readonly Dictionary<string, Func<IQueryable<User>, string, string, IQueryable<User>>> FilterColumns = new()
    {
        ["name"] = (q, op, val) => op switch
        {
            "$eq"    => q.Where(u => EF.Functions.ILike(u.Name, val)),
            "$not"   => q.Where(u => !EF.Functions.ILike(u.Name, val)),
            "$ilike" => q.Where(u => EF.Functions.ILike(u.Name, $"%{val}%")),
            "$sw"    => q.Where(u => EF.Functions.ILike(u.Name, $"{val}%")),
            _ => q
        },
        ["email"] = (q, op, val) => op switch
        {
            "$eq"    => q.Where(u => EF.Functions.ILike(u.Email, val)),
            "$not"   => q.Where(u => !EF.Functions.ILike(u.Email, val)),
            "$ilike" => q.Where(u => EF.Functions.ILike(u.Email, $"%{val}%")),
            "$sw"    => q.Where(u => EF.Functions.ILike(u.Email, $"{val}%")),
            _ => q
        },
        ["id"] = (q, op, val) =>
        {
            if (op == "$in") { var ids = FilterHelper.ParseInInt(val); return ids is null ? q : q.Where(u => ids.Contains(u.Id)); }
            if (!int.TryParse(val, out var n)) return q;
            return op switch
            {
                "$eq"  => q.Where(u => u.Id == n),
                "$gt"  => q.Where(u => u.Id > n),
                "$gte" => q.Where(u => u.Id >= n),
                "$lt"  => q.Where(u => u.Id < n),
                "$lte" => q.Where(u => u.Id <= n),
                _ => q
            };
        },
    };

    public async Task<PagedResponse<UserResponse>> GetAll(PageQuery q)
    {
        var query = db.Users.AsQueryable();

        // --- Filter ---
        query = query.ApplyFilters(q.Filters, FilterColumns);

        // --- Search ---
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(u => EF.Functions.ILike(u.Name, $"%{q.Search}%") || EF.Functions.ILike(u.Email, $"%{q.Search}%"));

        // --- Sort ---
        query = query.ApplySort(q.SortBy, SortColumns);

        var total = await query.CountAsync();
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(u => ToResponse(u))
            .ToListAsync();
        return new PagedResponse<UserResponse>(items, total, q.Page, q.PageSize);
    }

    public async Task<UserResponse?> GetById(int id)
    {
        var user = await db.Users.FindAsync(id);
        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponse> Create(CreateUserRequest request)
    {
        var user = new User { Name = request.Name, Email = request.Email };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<UpdateUserResult> Update(int id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return UpdateUserResult.NotFound();
        if (user.Version != request.Version) return UpdateUserResult.Conflict();

        user.Name = request.Name;
        user.Email = request.Email;
        user.Version++;

        await db.SaveChangesAsync();
        return UpdateUserResult.Success(ToResponse(user));
    }

    public async Task<bool> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return false;
        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    private static UserResponse ToResponse(User u) => new(u.Id, u.Name, u.Email, u.Version);
}
