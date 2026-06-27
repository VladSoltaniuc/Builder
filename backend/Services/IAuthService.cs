// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IAuthService
{
    Task<ProfileResponse> GetProfile(int userId);
    Task<RegisterResponse> Register(RegisterRequest request);
    Task VerifyEmail(string token);
    Task<LoginResponse> Login(LoginRequest request);
    Task<AuthResponse> LoginWithGoogle(GoogleLoginRequest request);
    Task<AuthResponse> VerifyTwoFactorLogin(TwoFactorLoginRequest request);
    Task<TwoFactorSetupResponse> SetupTwoFactor(int userId);
    Task EnableTwoFactor(int userId, string code);
    Task DisableTwoFactor(int userId, string code);
}
