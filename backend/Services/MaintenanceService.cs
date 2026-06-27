// Application layer — invokes the purge_audit stored procedure
using Npgsql;

namespace ProductApi.Services;

public class MaintenanceService(IConfiguration config) : IMaintenanceService
{
    private readonly string _connectionString =
        config.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

    public async Task PurgeAudit(int olderThanDays)
    {
        // A procedure is invoked with CALL (a function would be SELECT). We run it on a
        // raw connection with no explicit transaction, because the procedure COMMITs
        // internally — which isn't allowed inside an open transaction.
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("CALL purge_audit($1)", conn);
        cmd.Parameters.AddWithValue(olderThanDays);
        await cmd.ExecuteNonQueryAsync();
    }
}
