// Infrastructure layer — drains the email queue and sends each job, with bounded
// retry. This is the "something is watching" that a raw fire-and-forget lacks:
// one worker, controlled rate, and a few retries before giving up (and logging).
using Microsoft.Extensions.Logging;

namespace ProductApi.Reports;

public sealed class EmailQueueProcessor(
    IEmailQueue queue,
    IEmailSender sender,
    ILogger<EmailQueueProcessor> logger) : BackgroundService
{
    private const int MaxAttempts = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in queue.ReadAllAsync(stoppingToken))
            await SendWithRetryAsync(job, stoppingToken);
    }

    private async Task SendWithRetryAsync(EmailJob job, CancellationToken ct)
    {
        for (int attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await sender.SendAsync(job.To, job.Subject, job.HtmlBody, ct);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (attempt == MaxAttempts)
                {
                    logger.LogError(ex, "Giving up on email to {To} after {Attempts} attempts.", job.To, MaxAttempts);
                    return;
                }
                logger.LogWarning(ex, "Email to {To} failed (attempt {Attempt}/{Max}); retrying.", job.To, attempt, MaxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(2 * attempt), ct);
            }
        }
    }
}
