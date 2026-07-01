// Application layer
using ProductApi.Models;

namespace ProductApi.Auth;

public interface IJwtTokenService
{
    // Issues a signed JWT for the user and reports when it expires
    (string Token, DateTime ExpiresAt) CreateToken(User user);

    // Issues a short-lived token that only proves "password verified, 2FA pending"
    // It carries purpose=2fa and cannot be used as a normal bearer credential
    string CreatePendingTwoFactorToken(User user);

    // Validates a pending 2FA token and returns the user id it was issued for,
    // or null if the token is invalid, expired, or not a 2FA-purpose token
    int? ReadPendingTwoFactorUserId(string token);
}
