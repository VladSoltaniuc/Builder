// Application layer
using ProductApi.Models;

namespace ProductApi.Contracts;

public class ReportSubscriptionRequest
{
    public PreferredReportChannel Channel { get; set; }
    public string? PhoneNumber { get; set; }
}

