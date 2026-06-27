// Domain layer
namespace ProductApi.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // BCrypt hash. Empty for accounts that only sign in via an external provider.
    public string PasswordHash { get; set; } = string.Empty;

    // External login (e.g. "Google") and the provider's stable user id. Null for
    // password-only accounts.
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }

    // TOTP two-factor. Secret is a base32 string, set once the user opts in and
    // cleared when they disable. Enabled flips true only after a code is confirmed.
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }

    // Access level. New accounts are ReadOnly unless bootstrapped as the first Admin.
    public UserRole Role { get; set; } = UserRole.ReadOnly;

    // Opt-in for the weekly audit report email. Off by default.
    public bool WeeklyReportSubscribed { get; set; }

    public int Version { get; set; } = 1;
}
