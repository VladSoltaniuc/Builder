// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Auth;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class AuthService(AppDbContext db, IJwtTokenService tokens) : IAuthService
{
    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        var email = Normalize(request.Email);
        if (await db.Users.AnyAsync(u => u.Email == email))
            throw new UserFriendlyException("A user with this email already exists.", "CONFLICT");

        var user = new User
        {
            Name = request.Name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var email = Normalize(request.Email);
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);

        // Same error whether the email is unknown or the password is wrong, so the
        // response can't be used to probe which emails are registered.
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UserFriendlyException("Invalid email or password.", "UNAUTHORIZED");

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = tokens.CreateToken(user);
        return new AuthResponse(token, expiresAt, new UserResponse(user.Id, user.Name, user.Email, user.Version));
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
