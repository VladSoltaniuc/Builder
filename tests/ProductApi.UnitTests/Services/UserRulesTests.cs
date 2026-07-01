using FluentAssertions;
using ProductApi.Exceptions;
using ProductApi.Models;
using ProductApi.Services;

namespace ProductApi.UnitTests.Services;

public class UserRulesTests
{
    [Theory]
    [InlineData("  Alice@Example.COM  ", "alice@example.com")]
    [InlineData("BOB@X.IO", "bob@x.io")]
    [InlineData("already@lower.com", "already@lower.com")]
    public void NormalizeEmail_TrimsAndLowercases(string input, string expected)
        => UserRules.NormalizeEmail(input).Should().Be(expected);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RequirePhoneForSms_SmsWithoutPhone_ThrowsWithCode(string? phone)
    {
        var act = () => UserRules.RequirePhoneForSms(PreferredReportChannel.Sms, phone);

        act.Should().Throw<UserFriendlyException>()
            .Which.ErrorCode.Should().Be("PHONE_REQUIRED_FOR_SMS");
    }

    [Theory]
    [InlineData(PreferredReportChannel.Sms, "+15551234")]
    [InlineData(PreferredReportChannel.Email, null)]   // non-SMS channels never require a phone
    [InlineData(PreferredReportChannel.None, null)]
    public void RequirePhoneForSms_ValidCombination_DoesNotThrow(PreferredReportChannel channel, string? phone)
    {
        var act = () => UserRules.RequirePhoneForSms(channel, phone);

        act.Should().NotThrow();
    }
}
