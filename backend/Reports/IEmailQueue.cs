// Application layer
namespace ProductApi.Reports;

// A producer/consumer queue for outbound email. Producers enqueue and return
// immediately; a background consumer drains it at a controlled pace.
public interface IEmailQueue
{
    void Enqueue(EmailJob job);
    IAsyncEnumerable<EmailJob> ReadAllAsync(CancellationToken ct);
}
