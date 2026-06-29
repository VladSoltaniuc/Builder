// Application layer
using ProductApi.Constants;
using ProductApi.Models;

namespace ProductApi.Contracts;

// Weekly audit report delivery preference for the signed-in user. PhoneNumber is
// required when choosing Sms (unless one is already on file).
public class ReportSubscriptionRequest
{
    public PreferredReportChannel Channel { get; set; }
    public string? PhoneNumber { get; set; }
}

