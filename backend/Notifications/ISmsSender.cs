// Application layer
namespace ProductApi.Notifications;

public interface ISmsSender
{
    Task SendAsync(string toNumber, string message, CancellationToken ct = default);
}
