// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services;

public class UserService(AppDbContext db) : IUserService
{
    public async Task<PagedResponse<UserResponse>> GetAll(int page, int pageSize)
    {
        var total = await db.Users.CountAsync();
        var items = await db.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => ToResponse(u))
            .ToListAsync();
        return new PagedResponse<UserResponse>(items, total, page, pageSize);
    }

    public async Task<UserResponse?> GetById(int id)
    {
        var user = await db.Users.FindAsync(id);
        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponse> Create(CreateUserRequest request)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task<UpdateUserResult> Update(int id, UpdateUserRequest request)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return UpdateUserResult.NotFound();

        if (user.Version != request.Version)
            return UpdateUserResult.Conflict();

        user.Name = request.Name;
        user.Email = request.Email;
        user.Version++;

        await db.SaveChangesAsync();
        return UpdateUserResult.Success(ToResponse(user));
    }

    public async Task<bool> Delete(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
            return false;

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        return true;
    }

    private static UserResponse ToResponse(User u) =>
        new(u.Id, u.Name, u.Email, u.Version);
}
