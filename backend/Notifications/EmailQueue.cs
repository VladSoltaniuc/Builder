// Infrastructure layer
// In-memory channel: unbounded, single-reader, jobs lost on restart
using System.Threading.Channels;

namespace ProductApi.Notifications;

public sealed class EmailQueue : IEmailQueue
{
    private readonly Channel<EmailJob> _channel =
        Channel.CreateUnbounded<EmailJob>(new UnboundedChannelOptions { SingleReader = true });

    public void Enqueue(EmailJob job) => _channel.Writer.TryWrite(job);

    public IAsyncEnumerable<EmailJob> ReadAllAsync(CancellationToken ct) =>
        _channel.Reader.ReadAllAsync(ct);
}
