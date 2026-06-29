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
            ?? throw new UserFriendlyException("User not found.", "NOT_FOUND");

        // Can't text someone without a number â€” require one when choosing SMS.
        var phone = phoneNumber?.Trim();
        if (channel == PreferredReportChannel.Sms
            && string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(user.PhoneNumber))
            throw new UserFriendlyException("A phone number is required for SMS reports.", "INVALID_ARGUMENT");

        user.ReportChannel = channel;
        if (!string.IsNullOrWhiteSpace(phone))
            user.PhoneNumber = phone;

        await db.SaveChangesAsync();
    }
}

