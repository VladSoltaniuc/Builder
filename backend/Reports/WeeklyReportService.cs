// Infrastructure layer — weekly cron that emails the audit report to subscribers
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Services;

namespace ProductApi.Reports;

// Wakes once a week (default Monday 08:00 UTC), refreshes the audit metrics
// materialized view, then emails the report to every opted-in user.
//
// Mirrors IndexMaintenanceService's wait-until-next-occurrence loop: after a run
// it computes the following week's slot and sleeps until then, so each scheduled
// time fires once.
public sealed class WeeklyReportService(
    IServiceScopeFactory scopeFactory,
    IEmailSender emailSender,
    IOptions<WeeklyReportOptions> options,
    ILogger<WeeklyReportService> logger) : BackgroundService
{
    private readonly WeeklyReportOptions _opts = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opts.Enabled)
        {
            logger.LogInformation("Weekly report cron is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var next = _opts.NextOccurrence(DateTime.UtcNow);
            logger.LogInformation("Next weekly report scheduled for {Next:u} UTC.", next);

            if (!await WaitUntilAsync(next, stoppingToken))
                return; // cancelled

            try
            {
                await SendReportsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Weekly report run failed.");
            }
        }
    }

    // Sleeps until 'target' in capped chunks — a weekly gap exceeds Task.Delay's
    // ~24.8-day ceiling only for monthly waits, but the chunking also tolerates the
    // machine sleeping and clock drift.
    private static async Task<bool> WaitUntilAsync(DateTime target, CancellationToken ct)
    {
        var maxChunk = TimeSpan.FromHours(1);
        try
        {
            while (DateTime.UtcNow < target)
            {
                var remaining = target - DateTime.UtcNow;
                await Task.Delay(remaining < maxChunk ? remaining : maxChunk, ct);
            }
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task SendReportsAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var reports = scope.ServiceProvider.GetRequiredService<IReportService>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Refresh first so subscribers all read the same fresh snapshot of last week.
        await reports.RefreshWeeklyAuditReport();
        var rows = await reports.GetWeeklyAuditReport();

        var subscribers = await db.Users
            .Where(u => u.WeeklyReportSubscribed)
            .Select(u => new { u.Name, u.Email })
            .ToListAsync(ct);

        if (subscribers.Count == 0)
        {
            logger.LogInformation("Weekly report: no subscribers; nothing to send.");
            return;
        }

        var subject = $"Weekly audit report — week of {LastWeekLabel()}";
        var body = RenderHtml(rows);

        int sent = 0;
        foreach (var s in subscribers)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await emailSender.SendAsync(s.Email, subject, body, ct);
                sent++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // One bad address shouldn't stop the rest of the batch.
                logger.LogError(ex, "Failed to send weekly report to {Email}.", s.Email);
            }
        }

        logger.LogInformation("Weekly report sent to {Sent}/{Total} subscribers.", sent, subscribers.Count);
    }

    // Monday of last week, the start of the reporting window the view covers.
    private static string LastWeekLabel()
    {
        var today = DateTime.UtcNow.Date;
        int sinceMonday = ((int)today.DayOfWeek + 6) % 7; // Mon=0 .. Sun=6
        return today.AddDays(-sinceMonday - 7).ToString("yyyy-MM-dd");
    }

    private static string RenderHtml(List<WeeklyAuditReportResponse> rows)
    {
        var sb = new StringBuilder();
        sb.Append("<h2>Weekly audit report</h2>");
        sb.Append("<p>Changes recorded across the tracked tables last week:</p>");
        sb.Append("<table border=\"1\" cellpadding=\"6\" cellspacing=\"0\">");
        sb.Append("<tr><th>Table</th><th>Created</th><th>Updated</th><th>Deleted</th></tr>");
        foreach (var r in rows)
            sb.Append($"<tr><td>{r.TableName}</td><td>{r.Created}</td><td>{r.Updated}</td><td>{r.Deleted}</td></tr>");
        sb.Append("</table>");
        return sb.ToString();
    }
}
