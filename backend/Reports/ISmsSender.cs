// Application layer
namespace ProductApi.Reports;

public interface ISmsSender
{
    Task SendAsync(string toNumber, string message, CancellationToken ct = default);
}
