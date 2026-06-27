// Application layer
namespace ProductApi.Contracts;

// Result of a password login. When the account has 2FA enabled, Auth is null and
// the client must call /auth/2fa/verify with TwoFactorToken plus a TOTP code.
public record LoginResponse(bool RequiresTwoFactor, string? TwoFactorToken, AuthResponse? Auth)
{
    public static LoginResponse Authenticated(AuthResponse auth) => new(false, null, auth);
    public static LoginResponse TwoFactorRequired(string token) => new(true, token, null);
}
