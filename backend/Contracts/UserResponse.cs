// Application layer
using ProductApi.Constants;
using ProductApi.Models;

namespace ProductApi.Contracts;

public record UserResponse(
    int Id,
    string Name,
    string Email,
    string? PhoneNumber,
    PreferredReportChannel ReportChannel,
    UserRole Role,
    UserFeature Features,
    int Version);
