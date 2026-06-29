// Application layer
namespace ProductApi.Contracts;

public record TwoFactorSetupResponse(string Secret, string OtpAuthUri);
