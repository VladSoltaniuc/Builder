// Domain layer
namespace ProductApi.Models;

// How a user wants the weekly audit report delivered. None = not subscribed.
public enum PreferredReportChannel
{
    None,
    Email,
    Sms,
}
