// Application layer
namespace ProductApi.Contracts;

// Opt in/out of the weekly audit report email for the signed-in user.
public class ReportSubscriptionRequest
{
    public bool Subscribed { get; set; }
}
