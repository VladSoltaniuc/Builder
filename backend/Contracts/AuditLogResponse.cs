// Application layer
using System.Text.Json;

namespace ProductApi.Contracts;

public record AuditLogResponse(
    long Id,
    string TableName,
    string Action,
    int RowId,
    JsonElement? OldData,
    JsonElement? NewData,
    DateTime ChangedAt,
    string? ChangedBy);
