// Application layer
namespace ProductApi.Services;

public interface IMaintenanceService
{
    // Deletes audit rows older than the given number of days (in batches)
    Task PurgeAudit(int olderThanDays);
}
