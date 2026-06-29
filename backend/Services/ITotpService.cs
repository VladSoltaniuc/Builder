// Application layer
namespace ProductApi.Auth;

public interface ITotpService
{
    // Generates a fresh base32 TOTP secret.
    string GenerateSecret();

    // Builds the otpauth:// URI an authenticator app encodes as a QR code.
    string BuildOtpAuthUri(string secret, string accountEmail);

    // Validates a 6-digit code against the secret, allowing a one-step clock skew.
    bool Verify(string secret, string code);
}
