// Application layer
using ProductApi.Contracts;

namespace ProductApi.Services;

public interface IAuditService
{
    // Most recent audit entries, optionally filtered to one table and/or row.
    Task<List<AuditLogResponse>> GetHistory(string? table, int? rowId, int limit);
}
