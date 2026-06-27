// Application layer
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductApi.Auth;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Infrastructure;
using ProductApi.Models;

namespace ProductApi.Services;

public class AuthService(AppDbContext db, IJwtTokenService tokens, IOptions<GoogleAuthOptions> googleOptions) : IAuthService
{
    private const string GoogleProvider = "Google";
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

    public async Task<AuthResponse> LoginWithGoogle(GoogleLoginRequest request)
    {
        var clientId = googleOptions.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
            throw new UserFriendlyException("Google sign-in is not configured.", "UNAVAILABLE");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            // Verifies the token's signature, expiry, and that it was issued for us.
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });
        }
        catch (InvalidJwtException)
        {
            throw new UserFriendlyException("Invalid Google token.", "UNAUTHORIZED");
        }

        var email = Normalize(payload.Email);

        // Match by provider identity first, then fall back to email so an existing
        // password account gets linked rather than duplicated.
        var user = await db.Users.SingleOrDefaultAsync(u =>
            (u.ExternalProvider == GoogleProvider && u.ExternalId == payload.Subject) || u.Email == email);

        if (user is null)
        {
            user = new User { Name = payload.Name ?? email, Email = email };
            db.Users.Add(user);
        }

        // Link / refresh the external identity on the resolved account.
        user.ExternalProvider = GoogleProvider;
        user.ExternalId = payload.Subject;
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = tokens.CreateToken(user);
        return new AuthResponse(token, expiresAt, new UserResponse(user.Id, user.Name, user.Email, user.Version));
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
