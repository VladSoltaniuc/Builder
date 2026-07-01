// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Exceptions;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class UserService(AppDbContext db) : IUserService
{
    // Arbitrary application-wide key identifying the "last admin" advisory lock.
    private const long AdminGuardKey = 0x4C41_5354; // "LAST"

    private static readonly QueryShaper<User> Shaper = new QueryShaper<User>()
        .Sort("name",    u => u.Name)
        .Sort("email",   u => u.Email)
        .Search(u => u.Email);

    public async Task<PagedResponse<UserResponse>> GetAll(PageQuery q)
    {
        var query = db.Users.AsQueryable();

        query = Shaper.ApplySearch(query, q.Search);
        query = Shaper.ApplySort(query, q.SortBy);

        return await query.ToPagedResponse(q.Page, q.PageSize, u => ToResponse(u));
    }

    public async Task<UserResponse> GetById(int id)
    {
        var user = await db.Users.FindAsync(id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);
        return ToResponse(user);
    }

    public async Task<UserResponse> Create(CreateUserRequest request)
    {
        var phone = request.PhoneNumber?.Trim();
        UserRules.RequirePhoneForSms(request.ReportChannel, phone);

        var user = new User
        {
            Name = request.Name,
            Email = UserRules.NormalizeEmail(request.Email),
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
            ReportChannel = request.ReportChannel,
            Role = UserRole.Operator,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<UserResponse> Update(int id, UpdateUserRequest request)
    {
        // Don't delete last admin (advisory lock)
        await using var tx = await db.Database.BeginTransactionAsync();

        var user = await db.Users.FindAsync(id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);
        if (user.Version != request.Version)
            throw new UserFriendlyException("RESOURCE_CONFLICT", 409);

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
            await EnsureNotLastAdmin("LAST_ADMIN_CANNOT_DEMOTE");

        var phone = request.PhoneNumber?.Trim();
        UserRules.RequirePhoneForSms(request.ReportChannel, phone);

        user.Name = request.Name;
        user.Email = UserRules.NormalizeEmail(request.Email);
        user.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone;
        user.ReportChannel = request.ReportChannel;
        user.Role = request.Role;
        user.Features = request.Role == UserRole.Admin ? UserFeature.None : request.Features;
        user.Version++;

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return ToResponse(user);
    }

    public async Task Delete(int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var user = await db.Users.FindAsync(id)
            ?? throw new UserFriendlyException("RESOURCE_NOT_FOUND", 404);

        if (user.Role == UserRole.Admin)
            await EnsureNotLastAdmin("LAST_ADMIN_CANNOT_DELETE");

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    // Guards the "system must keep at least one Admin" invariant. The xact-scoped advisory
    // lock serializes concurrent demotions/deletions so two of them can't both read
    // count > 1 and commit, leaving zero admins. Held until the surrounding tx commits.
    private async Task EnsureNotLastAdmin(string code)
    {
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", AdminGuardKey);

        if (await db.Users.CountAsync(u => u.Role == UserRole.Admin) <= 1)
            throw new UserFriendlyException(code);
    }

    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.Email, u.PhoneNumber, u.ReportChannel, u.Role, u.Features, u.Version);
}
