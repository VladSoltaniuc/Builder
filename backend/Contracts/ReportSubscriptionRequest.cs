// Application layer
namespace ProductApi.Contracts;

// Weekly audit report delivery preferences for the signed-in user. Channels are
// independent — enable email, SMS, both, or neither. PhoneNumber is required when
// enabling SMS (unless one is already on file).
public class ReportSubscriptionRequest
{
    public bool Email { get; set; }
    public bool Sms { get; set; }
    public string? PhoneNumber { get; set; }
}
