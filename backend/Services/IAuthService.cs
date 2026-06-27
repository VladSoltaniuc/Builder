// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IAuthService
{
    Task<AuthResponse> Register(RegisterRequest request);
    Task<AuthResponse> Login(LoginRequest request);
}
