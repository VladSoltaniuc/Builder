// Domain layer
namespace ProductApi.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // BCrypt hash. Null for accounts that only sign in via an external provider.
    public string? PasswordHash { get; set; }

    // External login (e.g. "Google") and the provider's stable user id. Null for
    // password-only accounts.
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }

    // TOTP two-factor. Secret is a base32 string, set once the user opts in and
    // cleared when they disable. Enabled flips true only after a code is confirmed.
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }

    // Access level. New accounts are Operator unless bootstrapped as the founder Admin.
    public UserRole Role { get; set; } = UserRole.Operator;

    // Email verification. New password registrations start unverified and can't log
    // in until they click the emailed link. External (Google) logins are pre-verified.
    public bool EmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    // Weekly audit report delivery preference. None = not subscribed; Sms also
    // needs PhoneNumber set.
    public PreferredReportChannel ReportChannel { get; set; } = PreferredReportChannel.None;
    public string? PhoneNumber { get; set; }                // E.164, e.g. +14155552671

    // Optional capability grants layered on top of the Role. Admins ignore this —
    // the role already grants full access. Only meaningful for Operators.
    public UserFeature Features { get; set; } = UserFeature.None;

    public int Version { get; set; } = 1;
}

public enum UserRole
{
    Operator = 0,
    Admin    = 1,
}

[Flags]
public enum UserFeature
{
    None              = 0,
    CanExportExcel    = 1,
    CanViewAuditLog   = 2,
    CanManageInvoices = 4,
}

public enum PreferredReportChannel
{
    None  = 0,
    Email = 1,
    Sms   = 2,
}
