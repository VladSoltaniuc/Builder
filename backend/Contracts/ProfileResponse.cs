// Application layer
using ProductApi.Models;

namespace ProductApi.Contracts;

public record ProfileResponse(
    int Id,
    string Name,
    string Email,
    UserRole Role,
    UserFeature Features,
    string? PhoneNumber,
    PreferredReportChannel ReportChannel);
