// Infrastructure layer
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductApi.Configuration;
using ProductApi.Models;

namespace ProductApi.Data;

public static class AdminSeeder
{
    // Provisions the founder Admin if not already present
    // Set Enabled = true in appsettings to run
    public static async Task SeedAdminAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
        if (!options.Enabled)
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
