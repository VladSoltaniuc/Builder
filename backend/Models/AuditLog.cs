// Domain layer
namespace ProductApi.Models;

public class AuditLog
{
    public long Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // INSERT | UPDATE | DELETE
    public int RowId { get; set; }
    public string? OldData { get; set; }                // JSONB - row before (null on INSERT)
    public string? NewData { get; set; }                // JSONB - row after  (null on DELETE)
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }              // app.user_id, null until Auth exists
}
