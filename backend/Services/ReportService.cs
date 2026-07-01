// Application layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Exceptions;
using ProductApi.Models;

namespace ProductApi.Services;

public class ReportService(AppDbContext db) : IReportService
{
    public async Task<List<WeeklyAuditReportResponse>> GetWeeklyAuditReport()
        => await db.WeeklyAuditReport
            .OrderBy(r => r.TableName)
            .Select(r => new WeeklyAuditReportResponse(r.TableName, r.Inserts, r.Updates, r.Deletes))
            .ToListAsync();

    public Task RefreshWeeklyAuditReport()
        => db.Database.ExecuteSqlRawAsync("REFRESH MATERIALIZED VIEW mv_weekly_audit_report;");

    public async Task SetSubscription(int userId, PreferredReportChannel channel, string? phoneNumber)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new UserFriendlyException("USER_NOT_FOUND");

        var phone = phoneNumber?.Trim();
        UserRules.RequirePhoneForSms(channel, string.IsNullOrWhiteSpace(phone) ? user.PhoneNumber : phone);

        user.ReportChannel = channel;
        if (!string.IsNullOrWhiteSpace(phone))
            user.PhoneNumber = phone;

        await db.SaveChangesAsync();
    }
}

