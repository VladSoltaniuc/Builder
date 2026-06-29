// Infrastructure layer
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using ProductApi.Configuration;

namespace ProductApi.Notifications;

public sealed class TwilioSmsSender(IOptions<SmsOptions> options) : ISmsSender
{
    private readonly SmsOptions _opts = options.Value;

    public async Task SendAsync(string toNumber, string message, CancellationToken ct = default)
    {
        // Twilio's client uses a static, per-process credential store.
        TwilioClient.Init(_opts.AccountSid, _opts.AuthToken);

        await MessageResource.CreateAsync(
            to: new PhoneNumber(toNumber),
            from: new PhoneNumber(_opts.FromNumber),
            body: message);
    }
}
