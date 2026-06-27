// Application layer — reads/refreshes the weekly audit metrics materialized view
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Infrastructure;

namespace ProductApi.Services;

public class ReportService(AppDbContext db) : IReportService
{
    public async Task<List<WeeklyAuditReportResponse>> GetWeeklyAuditReport()
        => await db.WeeklyAuditReport
            .OrderBy(r => r.TableName)
            .Select(r => new WeeklyAuditReportResponse(r.TableName, r.Created, r.Updated, r.Deleted))
            .ToListAsync();

    public Task RefreshWeeklyAuditReport()
        => db.Database.ExecuteSqlRawAsync("REFRESH MATERIALIZED VIEW mv_weekly_audit_report;");

    public async Task SetSubscription(int userId, bool subscribed)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("User not found.", "NOT_FOUND");
        user.WeeklyReportSubscribed = subscribed;
        await db.SaveChangesAsync();
    }
}
