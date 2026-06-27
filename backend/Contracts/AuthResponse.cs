// Application layer
namespace ProductApi.Contracts;

public record AuthResponse(string Token, DateTime ExpiresAt, UserResponse User);
