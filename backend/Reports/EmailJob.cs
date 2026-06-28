// Application layer — a unit of work on the email queue.
namespace ProductApi.Reports;

public record EmailJob(string To, string Subject, string HtmlBody);
