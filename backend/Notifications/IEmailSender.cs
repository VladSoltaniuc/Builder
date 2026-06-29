// Application layer
namespace ProductApi.Notifications;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default);
}
