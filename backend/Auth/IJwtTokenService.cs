// Application layer
using ProductApi.Models;

namespace ProductApi.Auth;

public interface IJwtTokenService
{
    // Issues a signed JWT for the user and reports when it expires.
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
