// Application layer
namespace ProductApi.Reports;

public record EmailJob(string To, string Subject, string HtmlBody);
