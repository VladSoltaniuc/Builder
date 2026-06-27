// Application layer — reads/refreshes the weekly audit metrics materialized view
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;

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
}
