// Application layer
namespace ProductApi.Contracts;

public record WeeklyAuditReportResponse(string TableName, long Inserts, long Updates, long Deletes);
