// Application layer
namespace ProductApi.Contracts;

// Returned when a user begins 2FA enrollment. The client renders OtpAuthUri as a
// QR code; Secret is shown for manual entry.
public record TwoFactorSetupResponse(string Secret, string OtpAuthUri);
