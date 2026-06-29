// Domain layer
namespace ProductApi.Models;

public class WeeklyAuditReportView
{
    public string TableName { get; set; } = string.Empty;
    public long Inserts { get; set; }
    public long Updates { get; set; }
    public long Deletes { get; set; }
}
