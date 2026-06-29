// Application layer
namespace ProductApi.Notifications;

public record EmailJob(string To, string Subject, string HtmlBody);
