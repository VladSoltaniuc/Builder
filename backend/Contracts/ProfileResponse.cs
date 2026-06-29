// Application layer
using ProductApi.Constants;
using ProductApi.Models;

namespace ProductApi.Contracts;

// The signed-in user's own profile, returned by GET /api/auth/me.
public record ProfileResponse(
    int Id,
    string Name,
    string Email,
    UserRole Role,
    UserFeature Features,
    string? PhoneNumber,
    PreferredReportChannel ReportChannel);
