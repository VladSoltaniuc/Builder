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
        .Search(u => u.Email)
        .Sort("name",    u => u.Name)
        .Sort("email",   u => u.Email);

    public async Task<PagedResponse<UserResponse>> GetAll(PageQuery q)
    {
        var query = db.Users.AsQueryable();

        query = Shaper.ApplySearch(query, q.Search);
        query = Shaper.ApplySort(query, q.SortBy);

        return await query.ToPagedResponse(q.Page, q.PageSize, u => ToResponse(u));
    }

    public async Task<UserResponse?> GetById(int id)
    {
        var user = await db.Users.FindAsync(id);
        return user is null ? null : ToResponse(user);
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
            // All new Users start as Operators
            Role = UserRole.Operator,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<UpdateUserResult> Update(int id, UpdateUserRequest request)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var user = await db.Users.FindAsync(id);
        if (user is null) return UpdateUserResult.NotFound();
        if (user.Version != request.Version) return UpdateUserResult.Conflict();

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
            await EnsureNotLastAdmin(
                "Cannot demote the last Admin the system would have no admins.",
                "LAST_ADMIN_CANNOT_DEMOTE");

        var phone = request.PhoneNumber?.Trim();
        UserRules.RequirePhoneForSms(request.ReportChannel, phone);

        user.Name = request.Name;
        user.Email = UserRules.NormalizeEmail(request.Email);
        user.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone;
        user.ReportChannel = request.ReportChannel;
        user.Role = request.Role;
        // Admins always have full access features bits are meaningless for them
        // Clear on promotion so the DB stays clean; reset to None on any role change
        user.Features = request.Role == UserRole.Admin ? UserFeature.None : request.Features;
        user.Version++;

        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return UpdateUserResult.Success(ToResponse(user));
    }

    public async Task<bool> Delete(int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();

        var user = await db.Users.FindAsync(id);
        if (user is null) return false;

        if (user.Role == UserRole.Admin)
            await EnsureNotLastAdmin(
                "Cannot delete the last Admin the system would have no admins.",
                "LAST_ADMIN_CANNOT_DELETE");

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return true;
    }

    // Guards the "system must keep at least one Admin" invariant. The xact-scoped advisory
    // lock serializes concurrent demotions/deletions so two of them can't both read
    // count > 1 and commit, leaving zero admins. Held until the surrounding tx commits.
    private async Task EnsureNotLastAdmin(string message, string code)
    {
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", AdminGuardKey);

        if (await db.Users.CountAsync(u => u.Role == UserRole.Admin) <= 1)
            throw new UserFriendlyException(message, code);
    }

    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.Email, u.PhoneNumber, u.ReportChannel, u.Role, u.Features, u.Version);
}
