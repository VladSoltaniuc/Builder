// Application layer
using ProductApi.Exceptions;
using ProductApi.Models;

namespace ProductApi.Services;

public static class UserRules
{
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static void RequirePhoneForSms(PreferredReportChannel channel, string? effectivePhone)
    {
        if (channel == PreferredReportChannel.Sms && string.IsNullOrWhiteSpace(effectivePhone))
            throw new UserFriendlyException("A phone number is required for SMS reports.", "PHONE_REQUIRED_FOR_SMS");
    }
}
