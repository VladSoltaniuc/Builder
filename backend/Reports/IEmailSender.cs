// Application layer
namespace ProductApi.Reports;

public interface IEmailSender
{
    Task SendAsync(string toAddress, string subject, string htmlBody, CancellationToken ct = default);
}
