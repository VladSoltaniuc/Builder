// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IAuthService
{
    Task<AuthResponse> Register(RegisterRequest request);
    Task<LoginResponse> Login(LoginRequest request);
    Task<AuthResponse> LoginWithGoogle(GoogleLoginRequest request);
    Task<AuthResponse> VerifyTwoFactorLogin(TwoFactorLoginRequest request);
    Task<TwoFactorSetupResponse> SetupTwoFactor(int userId);
    Task EnableTwoFactor(int userId, string code);
    Task DisableTwoFactor(int userId, string code);
}
