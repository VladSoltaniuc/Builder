// Application layer
using System.Security.Cryptography;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductApi.Auth;
using ProductApi.Configuration;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Exceptions;
using ProductApi.Models;
using ProductApi.Notifications;

namespace ProductApi.Services;

public class AuthService(
    AppDbContext db,
    IJwtTokenService tokens,
    ITotpService totp,
    IEmailQueue emailQueue,
    IOptions<GoogleAuthOptions> googleOptions,
    IOptions<AppOptions> appOptions) : IAuthService
{
    // #Note: Maybe I can take these 2 out in defaults?
    private const string GoogleProvider = "Google";
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    public async Task<RegisterResponse> Register(RegisterRequest request)
    {
        var email = UserRules.NormalizeEmail(request.Email);
        if (await db.Users.AnyAsync(u => u.Email == email))
            throw new UserFriendlyException("EMAIL_ALREADY_EXISTS");
        var token = GenerateVerificationToken();
        var user = new User
        {
            Name = request.Name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password!),
            Role = UserRole.Operator,
            EmailVerified = false,
            EmailVerificationToken = token,
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.Add(VerificationTokenLifetime),
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        QueueVerificationEmail(user.Name, user.Email, token);

        return new RegisterResponse(email, "Verification email sent. Please check your inbox.");
    }

    public async Task VerifyEmail(string token)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.EmailVerificationToken == token);
        if (user is null || user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
            throw new UserFriendlyException("VERIFICATION_LINK_INVALID");

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiresAt = null;
        await db.SaveChangesAsync();
    }

    // Cryptographically random, no padding/reserved chars
    private static string GenerateVerificationToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

    private void QueueVerificationEmail(string name, string email, string token)
    {
        var link = $"{appOptions.Value.FrontendUrl.TrimEnd('/')}/verify-email?token={token}";
        var html = $"""
            <h2>Confirm your email</h2>
            <p>Hi {name}, thanks for registering. Please confirm your email address to activate your account.</p>
            <p><a href="{link}" style="display:inline-block;padding:10px 18px;background:#2563eb;color:#fff;text-decoration:none;border-radius:6px">Verify email</a></p>
            <p>Or open this link in your browser:<br>{link}</p>
            <p>This link expires in 24 hours.</p>
            """;

        // Hand off to the queue and return a background worker sends it, with retry
        emailQueue.Enqueue(new EmailJob(email, "Verify your email", html));
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var email = UserRules.NormalizeEmail(request.Email);
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);

        if (user is null || user.PasswordHash is null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UserFriendlyException("INVALID_CREDENTIALS");

        if (!user.EmailVerified)
            throw new UserFriendlyException("EMAIL_NOT_VERIFIED");

        if (user.TwoFactorEnabled)
            return LoginResponse.TwoFactorRequired(tokens.CreatePendingTwoFactorToken(user));

        return LoginResponse.Authenticated(BuildResponse(user));
    }

    public async Task<AuthResponse> VerifyTwoFactorLogin(TwoFactorLoginRequest request)
    {
        var userId = tokens.ReadPendingTwoFactorUserId(request.TwoFactorToken)
            ?? throw new UserFriendlyException("TWO_FACTOR_SESSION_EXPIRED");

        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("TWO_FACTOR_SESSION_EXPIRED");

        if (!user.TwoFactorEnabled || !totp.Verify(user.TwoFactorSecret!, request.Code))
            throw new UserFriendlyException("INVALID_AUTH_CODE");

        return BuildResponse(user);
    }

    public async Task<TwoFactorSetupResponse> SetupTwoFactor(int userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("USER_NOT_FOUND");

        // Store the candidate secret but leave 2FA disabled until a code confirms it
        var secret = totp.GenerateSecret();
        user.TwoFactorSecret = secret;
        user.TwoFactorEnabled = false;
        await db.SaveChangesAsync();

        return new TwoFactorSetupResponse(secret, totp.BuildOtpAuthUri(secret, user.Email));
    }

    public async Task EnableTwoFactor(int userId, string code)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("USER_NOT_FOUND");

        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            throw new UserFriendlyException("TWO_FACTOR_SETUP_REQUIRED");
        if (!totp.Verify(user.TwoFactorSecret, code))
            throw new UserFriendlyException("INVALID_AUTH_CODE");

        user.TwoFactorEnabled = true;
        await db.SaveChangesAsync();
    }

    public async Task DisableTwoFactor(int userId, string code)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("USER_NOT_FOUND");

        if (!user.TwoFactorEnabled || !totp.Verify(user.TwoFactorSecret!, code))
            throw new UserFriendlyException("INVALID_AUTH_CODE");

        user.TwoFactorEnabled = false;
        user.TwoFactorSecret = null;
        await db.SaveChangesAsync();
    }

    public async Task<AuthResponse> LoginWithGoogle(GoogleLoginRequest request)
    {
        var clientId = googleOptions.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
            throw new UserFriendlyException("GOOGLE_SIGNIN_UNAVAILABLE");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            // Verifies the token's signature, expiry, and that it was issued for us
            payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });
        }
        catch (InvalidJwtException)
        {
            throw new UserFriendlyException("INVALID_GOOGLE_TOKEN");
        }

        var email = UserRules.NormalizeEmail(payload.Email);
        var user = await db.Users.SingleOrDefaultAsync(u =>
            (u.ExternalProvider == GoogleProvider && u.ExternalId == payload.Subject) || u.Email == email);

        if (user is null)
        {
            user = new User { Name = payload.Name ?? email, Email = email };
            db.Users.Add(user);
        }

        user.ExternalProvider = GoogleProvider;
        user.ExternalId = payload.Subject;
        user.EmailVerified = true;
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<ProfileResponse> GetProfile(int userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("USER_NOT_FOUND");
        return new ProfileResponse(user.Id, user.Name, user.Email, user.Role, user.Features, user.PhoneNumber, user.ReportChannel);
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = tokens.CreateToken(user);
        return new AuthResponse(token, expiresAt,
            new UserResponse(user.Id, user.Name, user.Email, user.PhoneNumber, user.ReportChannel, user.Role, user.Features, user.Version));
    }
}
