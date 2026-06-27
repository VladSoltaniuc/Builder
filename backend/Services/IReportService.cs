// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IReportService
{
    // Reads the precomputed weekly audit metrics (one row per audited table).
    Task<List<WeeklyAuditReportResponse>> GetWeeklyAuditReport();

    // Recomputes the materialized view from the current AuditLogs. Called by the
    // weekly cron just before the report goes out.
    Task RefreshWeeklyAuditReport();

    // Opts a user in or out of the weekly report email.
    Task SetSubscription(int userId, bool subscribed);
}
