// Infrastructure layer
using OtpNet;

namespace ProductApi.Auth;

public class TotpService : ITotpService
{
    private const string Issuer = "ProductApi";

    public string GenerateSecret()
        => Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

    public string BuildOtpAuthUri(string secret, string accountEmail)
    {
        var label = Uri.EscapeDataString($"{Issuer}:{accountEmail}");
        return $"otpauth://totp/{label}?secret={secret}&issuer={Uri.EscapeDataString(Issuer)}&digits=6&period=30";
    }

    public bool Verify(string secret, string code)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
            return false;

        var totp = new Totp(Base32Encoding.ToBytes(secret));
        return totp.VerifyTotp(code.Trim(), out _, VerificationWindow.RfcSpecifiedNetworkDelay);
    }
}
