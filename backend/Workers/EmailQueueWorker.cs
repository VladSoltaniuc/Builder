// Infrastructure layer
using Microsoft.Extensions.Options;
using ProductApi.Configuration;
using ProductApi.Notifications;

namespace ProductApi.Workers;

public sealed class EmailQueueWorker(
    IEmailQueue queue,
    IEmailSender sender,
    IOptions<EmailOptions> options,
    ILogger<EmailQueueWorker> logger) : BackgroundService
{
    private readonly int _maxAttempts = options.Value.MaxRetries;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in queue.ReadAllAsync(stoppingToken))
            await SendWithRetryAsync(job, stoppingToken);
    }

    private async Task SendWithRetryAsync(EmailJob job, CancellationToken ct)
    {
        for (int attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                await sender.SendAsync(job.To, job.Subject, job.HtmlBody, ct);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (attempt == _maxAttempts)
                {
                    logger.LogError(ex, "Giving up on email to {To} after {Attempts} attempts.", job.To, _maxAttempts);
                    return;
                }
                logger.LogWarning(ex, "Email to {To} failed (attempt {Attempt}/{Max}); retrying.", job.To, attempt, _maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct);
            }
        }
    }
}
