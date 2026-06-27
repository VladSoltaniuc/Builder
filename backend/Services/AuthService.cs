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

public class AuthService(AppDbContext db, IJwtTokenService tokens, ITotpService totp, IOptions<GoogleAuthOptions> googleOptions) : IAuthService
{
    private const string GoogleProvider = "Google";
    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        var email = Normalize(request.Email);
        if (await db.Users.AnyAsync(u => u.Email == email))
            throw new UserFriendlyException("A user with this email already exists.", "CONFLICT");

        // Bootstrap: if no admin exists yet, the first person to register becomes one.
        // Everyone else starts read-only and must be promoted by an admin.
        var role = await db.Users.AnyAsync(u => u.Role == UserRole.Admin)
            ? UserRole.ReadOnly
            : UserRole.Admin;

        var user = new User
        {
            Name = request.Name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var email = Normalize(request.Email);
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);

        // Same error whether the email is unknown or the password is wrong, so the
        // response can't be used to probe which emails are registered.
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UserFriendlyException("Invalid email or password.", "UNAUTHORIZED");

        // Password is correct, but if 2FA is on we issue only a short-lived pending
        // token and withhold the real one until the TOTP code is verified.
        if (user.TwoFactorEnabled)
            return LoginResponse.TwoFactorRequired(tokens.CreatePendingTwoFactorToken(user));

        return LoginResponse.Authenticated(BuildResponse(user));
    }

    public async Task<AuthResponse> VerifyTwoFactorLogin(TwoFactorLoginRequest request)
    {
        var userId = tokens.ReadPendingTwoFactorUserId(request.TwoFactorToken)
            ?? throw new UserFriendlyException("Two-factor session expired. Please sign in again.", "UNAUTHORIZED");

        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("Two-factor session expired. Please sign in again.", "UNAUTHORIZED");

        if (!user.TwoFactorEnabled || !totp.Verify(user.TwoFactorSecret!, request.Code))
            throw new UserFriendlyException("Invalid authentication code.", "UNAUTHORIZED");

        return BuildResponse(user);
    }

    public async Task<TwoFactorSetupResponse> SetupTwoFactor(int userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("User not found.", "NOT_FOUND");

        // Store the candidate secret but leave 2FA disabled until a code confirms it.
        var secret = totp.GenerateSecret();
        user.TwoFactorSecret = secret;
        user.TwoFactorEnabled = false;
        await db.SaveChangesAsync();

        return new TwoFactorSetupResponse(secret, totp.BuildOtpAuthUri(secret, user.Email));
    }

    public async Task EnableTwoFactor(int userId, string code)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("User not found.", "NOT_FOUND");

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new UserFriendlyException("Start two-factor setup first.", "INVALID_ARGUMENT");
        if (!totp.Verify(user.TwoFactorSecret, code))
            throw new UserFriendlyException("Invalid authentication code.", "UNAUTHORIZED");

        user.TwoFactorEnabled = true;
        await db.SaveChangesAsync();
    }

    public async Task DisableTwoFactor(int userId, string code)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("User not found.", "NOT_FOUND");

        if (!user.TwoFactorEnabled || !totp.Verify(user.TwoFactorSecret!, code))
            throw new UserFriendlyException("Invalid authentication code.", "UNAUTHORIZED");

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        await db.SaveChangesAsync();
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

    public async Task<ProfileResponse> GetProfile(int userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("User not found.", "NOT_FOUND");
        return new ProfileResponse(user.Id, user.Name, user.Email, user.Role, user.PhoneNumber, user.ReportChannel);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = tokens.CreateToken(user);
        return new AuthResponse(token, expiresAt,
            new UserResponse(user.Id, user.Name, user.Email, user.PhoneNumber, user.ReportChannel, user.Version));
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
