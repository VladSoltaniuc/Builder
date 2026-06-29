// Application layer
namespace ProductApi.Contracts;

public record LoginResponse(bool RequiresTwoFactor, string? TwoFactorToken, AuthResponse? Auth)
{
    public static LoginResponse Authenticated(AuthResponse auth) => new(false, null, auth);
    public static LoginResponse TwoFactorRequired(string token) => new(true, token, null);
}
