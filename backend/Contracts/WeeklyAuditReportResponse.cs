// Application layer
namespace ProductApi.Contracts;

public record WeeklyAuditReportResponse(string TableName, long Created, long Updated, long Deleted);
