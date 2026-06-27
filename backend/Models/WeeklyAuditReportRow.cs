// Domain layer — a keyless row read from the mv_weekly_audit_report materialized
// view. One row per audited table, counting last week's changes by action.
namespace ProductApi.Models;

public class WeeklyAuditReportRow
{
    public string TableName { get; set; } = string.Empty;
    public long Created { get; set; } // INSERTs
    public long Updated { get; set; } // UPDATEs
    public long Deleted { get; set; } // DELETEs
}
