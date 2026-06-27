// Infrastructure layer — stamps the signed-in user's id onto the DB session so the
// audit triggers can record "who" in AuditLogs.ChangedBy.
//
// The triggers read current_setting('app.user_id', true). We set it (session-scoped,
// is_local=false) on the same open connection EF uses for the save, so it's visible
// to every INSERT/UPDATE/DELETE in that SaveChanges. Npgsql resets connection state
// on return to the pool, so the value can't leak into another request.
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProductApi.Data;

public sealed class AuditUserInterceptor(IHttpContextAccessor http) : SaveChangesInterceptor
{
    private const string SetUserSql = "SELECT set_config('app.user_id', @userId, false)";

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUserId();
        if (userId is not null && eventData.Context is { } ctx)
        {
            var connection = ctx.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync(cancellationToken);

            await using var cmd = connection.CreateCommand();
            cmd.CommandText = SetUserSql;
            AddUserParam(cmd, userId);
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        var userId = CurrentUserId();
        if (userId is not null && eventData.Context is { } ctx)
        {
            var connection = ctx.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = SetUserSql;
            AddUserParam(cmd, userId);
            cmd.ExecuteNonQuery();
        }

        return base.SavingChanges(eventData, result);
    }

    private string? CurrentUserId()
    {
        var id = http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrEmpty(id) ? null : id;
    }

    private static void AddUserParam(System.Data.Common.DbCommand cmd, string userId)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = "@userId";
        p.Value = userId;
        cmd.Parameters.Add(p);
    }
}
