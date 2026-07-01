// Application layer
using ProductApi.Contracts;
using ProductApi.Models;

namespace ProductApi.Services;

public interface IReportService
{
    // Reads the precomputed weekly audit metrics (one row per audited table)
    Task<List<WeeklyAuditReportResponse>> GetWeeklyAuditReport();

    // Recomputes the materialized view from the current AuditLogs. Called by the
    // weekly cron just before the report goes out
    Task RefreshWeeklyAuditReport();

    // Sets a user's weekly report delivery channel (and phone number for SMS)
    Task SetSubscription(int userId, PreferredReportChannel channel, string? phoneNumber);
}

