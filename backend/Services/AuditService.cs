// Application layer
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductApi.Contracts;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Services;

public class AuditService(AppDbContext db) : IAuditService
{
    private const int MaxLimit = 200;

    public async Task<List<AuditLogResponse>> GetHistory(string? table, int? rowId, int limit)
    {
        limit = Math.Clamp(limit, 1, MaxLimit);
        var query = db.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(table))
            query = query.Where(a => a.TableName == table);
        if (rowId is not null)
            query = query.Where(a => a.RowId == rowId);

        var rows = await query
            .OrderByDescending(a => a.Id)
            .Take(limit)
            .ToListAsync();

        return rows.Select(ToResponse).ToList();
    }

    private static AuditLogResponse ToResponse(AuditLog a) =>
        new(a.Id, a.TableName, a.Action, a.RowId, Parse(a.OldData), Parse(a.NewData), a.ChangedAt, a.ChangedBy);

    // The JSONB columns come back as text; surface them as real JSON in the response
    private static JsonElement? Parse(string? json) =>
        json is null ? null : JsonSerializer.Deserialize<JsonElement>(json);
}
