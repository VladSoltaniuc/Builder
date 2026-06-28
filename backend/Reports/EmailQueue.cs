// Infrastructure layer — in-process email queue backed by System.Threading.Channels.
//
// Unbounded + single-reader: producers never block, and exactly one background
// consumer drains it. In-memory only — pending jobs are lost on restart; swap the
// channel for a durable broker (RabbitMQ/SQS) if that ever matters.
using System.Threading.Channels;

namespace ProductApi.Reports;

public sealed class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailJob> _channel =
        Channel.CreateUnbounded<EmailJob>(new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(EmailJob job) => _channel.Writer.TryWrite(job);

    public IAsyncEnumerable<EmailJob> ReadAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
