// Application layer
namespace ProductApi.Notifications;

// Producer/consumer queue for outbound email; a background consumer drains it.
public interface IEmailQueue
{
    void Enqueue(EmailJob job);
    IAsyncEnumerable<EmailJob> ReadAllAsync(CancellationToken ct);
}
