// Infrastructure layer — provisions the configured founder Admin at startup.
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductApi.Auth;
using ProductApi.Models;

namespace ProductApi.Data;

public static class AdminSeeder
{
    // Ensures the configured Admin exists. No-ops when AdminSeed isn't configured
    // (so tests and un-provisioned environments are untouched), and is idempotent —
    // it only inserts when that email is missing, never overwrites.
    public static async Task SeedAdminAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
        if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
            return;

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = options.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email))
            return;

        db.Users.Add(new User
        {
            Name = options.Name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(options.Password),
            Role = UserRole.Admin,
            EmailVerified = true,
        });
        await db.SaveChangesAsync();
    }
}
