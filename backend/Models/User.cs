// Domain layer
namespace ProductApi.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // Null if sign in via external
    public string? PasswordHash { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    // New accounts are always Operator, can upgrade to Admin.
    public UserRole Role { get; set; } = UserRole.Operator;
    public bool EmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }
    // Weekly audit report delivery preference
    public PreferredReportChannel ReportChannel { get; set; } = PreferredReportChannel.None;
    public string? PhoneNumber { get; set; }
    // Extra features for operators
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
