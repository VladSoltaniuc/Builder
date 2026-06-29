// Infrastructure layer
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Models;
using ProductApi.Configuration;
using ProductApi.Notifications;
using ProductApi.Services;

namespace ProductApi.Workers;

// Wakes once a week (default Monday 08:00 UTC), refreshes the audit metrics
// materialized view, then emails the report to every opted-in user.
//
// Mirrors IndexMaintenanceWorker's wait-until-next-occurrence loop: after a run
// it computes the following week's slot and sleeps until then, so each scheduled
// time fires once.
public sealed class WeeklyReportWorker(
    IServiceScopeFactory scopeFactory,
    IEmailSender emailSender,
    ISmsSender smsSender,
    IOptions<WeeklyReportOptions> options,
    ILogger<WeeklyReportWorker> logger) : BackgroundService
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

        // Anyone with a delivery channel selected.
        var subscribers = await db.Users
            .Where(u => u.ReportChannel != PreferredReportChannel.None)
            .Select(u => new { u.Email, u.PhoneNumber, u.ReportChannel })
            .ToListAsync(ct);

        if (subscribers.Count == 0)
        {
            logger.LogInformation("Weekly report: no subscribers; nothing to send.");
            return;
        }

        var label = LastWeekLabel();
        var subject = $"Weekly audit report — week of {label}";
        var html = RenderHtml(rows);
        var sms = RenderSms(rows, label);

        // Fire all sends concurrently — each subscriber's network I/O overlaps instead
        // of queuing behind the previous one. TrySend isolates failures per recipient.
        var emailTasks = subscribers
            .Where(s => s.ReportChannel == PreferredReportChannel.Email)
            .Select(s => TrySend(() => emailSender.SendAsync(s.Email, subject, html, ct), "email", s.Email));

        var smsTasks = subscribers
            .Where(s => s.ReportChannel == PreferredReportChannel.Sms && !string.IsNullOrWhiteSpace(s.PhoneNumber))
            .Select(s => TrySend(() => smsSender.SendAsync(s.PhoneNumber!, sms, ct), "SMS", s.PhoneNumber!));

        var emailSent = (await Task.WhenAll(emailTasks)).Sum();
        ct.ThrowIfCancellationRequested();
        var smsSent   = (await Task.WhenAll(smsTasks)).Sum();

        logger.LogInformation("Weekly report delivered: {Email} emails, {Sms} texts.", emailSent, smsSent);
    }

    // Sends one message, isolating failures so one bad recipient never stops the batch.
    // Returns 1 on success, 0 on a logged failure.
    private async Task<int> TrySend(Func<Task> send, string channel, string recipient)
    {
        try
        {
            await send();
            return 1;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to send weekly report {Channel} to {Recipient}.", channel, recipient);
            return 0;
        }
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
            sb.Append($"<tr><td>{r.TableName}</td><td>{r.Inserts}</td><td>{r.Updates}</td><td>{r.Deletes}</td></tr>");
        sb.Append("</table>");
        return sb.ToString();
    }

    // Compact plain-text summary for SMS — one line per table, created/updated/deleted.
    private static string RenderSms(List<WeeklyAuditReportResponse> rows, string weekLabel)
    {
        var sb = new StringBuilder();
        sb.Append($"Weekly audit (week of {weekLabel}) created/updated/deleted: ");
        sb.Append(string.Join("; ", rows.Select(r => $"{r.TableName} {r.Inserts}/{r.Updates}/{r.Deletes}")));
        return sb.ToString();
    }
}
