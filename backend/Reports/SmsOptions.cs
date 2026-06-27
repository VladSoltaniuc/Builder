// Infrastructure layer — bound from the "Sms" configuration section (Twilio).
namespace ProductApi.Reports;

public sealed class SmsOptions
{
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty; // E.164, e.g. +14155552671
}
