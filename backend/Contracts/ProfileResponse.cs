// Application layer
using ProductApi.Models;

namespace ProductApi.Contracts;

// The signed-in user's own profile, returned by GET /api/auth/me.
public record ProfileResponse(
    int Id,
    string Name,
    string Email,
    UserRole Role,
    string? PhoneNumber,
    PreferredReportChannel ReportChannel);
