// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Constants;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class UserService(AppDbContext db) : IUserService
{
    private static readonly EntityFilter<User> Filter = new EntityFilter<User>()
        .String("name",  u => u.Name)
        .String("email", u => u.Email)
        .Int("id",       u => u.Id)
        .Sort("name",    u => u.Name)
        .Sort("email",   u => u.Email);

    public async Task<PagedResponse<UserResponse>> GetAll(PageQuery q)
    {
        var query = db.Users.AsQueryable();

        // --- Filter ---
        query = Filter.Apply(query, q.Filters);

        // --- Search ---
        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(u => EF.Functions.ILike(u.Name, $"%{q.Search}%") || EF.Functions.ILike(u.Email, $"%{q.Search}%"));

        // --- Sort ---
        query = Filter.ApplySort(query, q.SortBy);

        return await query.ToPagedResponse(q.Page, q.PageSize, u => ToResponse(u));
    }

    // Substring search over Name and Email, backed by pg_trgm GIN indexes (ILIKE '%term%').
    public async Task<List<UserResponse>> Search(string term)
    {
        var pattern = $"%{term}%";
        return await db.Users
            .AsNoTracking()
            .Where(u => EF.Functions.ILike(u.Name, pattern) || EF.Functions.ILike(u.Email, pattern))
            .OrderBy(u => u.Id)
            .Take(SearchDefaults.MaxResults)
            .Select(u => ToResponse(u))
            .ToListAsync();
    }

    public async Task<UserResponse?> GetById(int id)
    {
        var user = await db.Users.FindAsync(id);
        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponse> Create(CreateUserRequest request)
    {
        var phone = request.PhoneNumber?.Trim();
        ValidateChannel(request.ReportChannel, phone);

        var user = new User
        {
            Name = request.Name,
            Email = Normalize(request.Email),
            PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone,
            ReportChannel = request.ReportChannel,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<UpdateUserResult> Update(int id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return UpdateUserResult.NotFound();
        if (user.Version != request.Version) return UpdateUserResult.Conflict();

        // Prevent demoting the last Admin — would lock everyone out.
        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin)
        {
            var adminCount = await db.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
                throw new UserFriendlyException(
                    "Cannot demote the last Admin — the system would have no admins.",
                    "LAST_ADMIN");
        }

        var phone = request.PhoneNumber?.Trim();
        ValidateChannel(request.ReportChannel, phone);

        user.Name = request.Name;
        user.Email = Normalize(request.Email);
        user.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone;
        user.ReportChannel = request.ReportChannel;
        user.Role = request.Role;
        // Admins always have full access — features bits are meaningless for them.
        // Clear on promotion so the DB stays clean; reset to None on any role change.
        user.Features = request.Role == UserRole.Admin ? UserFeature.None : request.Features;
        user.Version++;

        await db.SaveChangesAsync();
        return UpdateUserResult.Success(ToResponse(user));
    }

    public async Task<bool> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null) return false;

        if (user.Role == UserRole.Admin)
        {
            var adminCount = await db.Users.CountAsync(u => u.Role == UserRole.Admin);
            if (adminCount <= 1)
                throw new UserFriendlyException(
                    "Cannot delete the last Admin — the system would have no admins.",
                    "LAST_ADMIN");
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    // Trim + lower-case so "Alice@X.com" and "alice@x.com" can't both exist.
    private static string Normalize(string email) => email.Trim().ToLowerInvariant();

    // SMS delivery needs somewhere to send to.
    private static void ValidateChannel(PreferredReportChannel channel, string? phone)
    {
        if (channel == PreferredReportChannel.Sms && string.IsNullOrWhiteSpace(phone))
            throw new UserFriendlyException("A phone number is required for SMS reports.", "INVALID_ARGUMENT");
    }

    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.Email, u.PhoneNumber, u.ReportChannel, u.Role, u.Features, u.Version);
}
