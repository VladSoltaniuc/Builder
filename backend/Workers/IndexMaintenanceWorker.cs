// Infrastructure layer
using Microsoft.Extensions.Options;
using Npgsql;
using ProductApi.Configuration;

namespace ProductApi.Workers;

// Wakes on a timer, measures index bloat with pgstattuple, and rebuilds ONLY the
// indexes that have actually bloated.
//
// Why a raw connection instead of EF: REINDEX ... CONCURRENTLY is illegal inside a
// transaction block, and that's exactly why it can't live in a stored procedure.
// We run each REINDEX as a single statement on its own NpgsqlConnection with no
// explicit transaction (autocommit), which is the one context Postgres allows it in.
public sealed class IndexMaintenanceWorker(
    IConfiguration config,
    IOptions<IndexMaintenanceOptions> options,
    ILogger<IndexMaintenanceWorker> logger) : BackgroundService
{
    private readonly IndexMaintenanceOptions _opts = options.Value;
    private readonly string _connectionString =
        config.GetConnectionString("Default")
        ?? throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

    // Read-only bloat report, scoped to our three app tables. pgstattuple scans each
    // index, so the size gate keeps us from scanning/rebuilding trivially small ones.
    //
    // btree only: pgstattuple cannot inspect GIN indexes, and GIN doesn't need this kind
    // of reindexing anyway — its bloat is the "pending list", which autovacuum flushes.
    // btree is what fragments under churn and what REINDEX actually helps.
    private const string BloatQuery = """
        SELECT (i.indexrelid::regclass)::text     AS index_name,
               s.free_percent                     AS free_percent,
               pg_relation_size(i.indexrelid)     AS size_bytes
        FROM pg_index i
        JOIN pg_class c     ON c.oid = i.indexrelid
        JOIN pg_class t     ON t.oid = i.indrelid
        JOIN pg_namespace n ON n.oid = c.relnamespace
        JOIN pg_am am       ON am.oid = c.relam
        CROSS JOIN LATERAL pgstattuple(i.indexrelid) s
        WHERE n.nspname = 'public'
          AND t.relname IN ('Products', 'Users', 'Orders')
          AND am.amname = 'btree'
          AND pg_relation_size(i.indexrelid) >= @minBytes
          AND s.free_percent >= @threshold;
        """;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opts.Enabled)
        {
            logger.LogInformation("Index maintenance is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var next = _opts.NextOccurrence(DateTime.UtcNow);
            logger.LogInformation("Next index maintenance scheduled for {Next:u} UTC.", next);

            if (!await WaitUntilAsync(next, stoppingToken))
                return; // cancelled

            try
            {
                await ReindexBloatedAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Index maintenance run failed.");
            }
        }
    }

    // Sleeps until 'target' in capped chunks — a monthly gap exceeds Task.Delay's
    // ~24.8-day ceiling, and short hops also tolerate the machine sleeping/clock drift.
    private static async Task<bool> WaitUntilAsync(DateTime target, CancellationToken ct)
    {
        var maxChunk = TimeSpan.FromHours(1);
        try
        {
            while (DateTime.UtcNow < target)
            {
                var remaining = target - DateTime.UtcNow;
                await Task.Delay(remaining < maxChunk ? remaining : maxChunk, ct);
            }
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private async Task ReindexBloatedAsync(CancellationToken ct)
    {
        var bloated = await FindBloatedIndexesAsync(ct);

        if (bloated.Count == 0)
        {
            logger.LogInformation(
                "Index maintenance: nothing to do (no index over {Threshold}% free, min size {MinSize} MB).",
                _opts.BloatThresholdPercent, _opts.MinIndexSizeMb);
            return;
        }

        foreach (var (name, freePct, sizeBytes) in bloated)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync(ct);
                // No transaction here on purpose — CONCURRENTLY requires autocommit.
                await using var cmd = new NpgsqlCommand($"REINDEX INDEX CONCURRENTLY {name}", conn)
                {
                    CommandTimeout = 0 // a rebuild can run long; don't impose a timeout
                };
                await cmd.ExecuteNonQueryAsync(ct);
                logger.LogInformation(
                    "Reindexed {Index} (was {Free:F1}% free, {Size:N0} bytes).", name, freePct, sizeBytes);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // A failed CONCURRENTLY build can leave an INVALID index behind that
                // must be dropped/rebuilt by hand — surface it loudly, keep going.
                logger.LogError(ex,
                    "Failed to reindex {Index}; it may be left INVALID and need a manual REINDEX.", name);
            }
        }
    }

    private async Task<List<(string Name, double FreePct, long SizeBytes)>> FindBloatedIndexesAsync(CancellationToken ct)
    {
        var minBytes = (long)(_opts.MinIndexSizeMb * 1024 * 1024);
        var results = new List<(string, double, long)>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(BloatQuery, conn);
        cmd.Parameters.AddWithValue("minBytes", minBytes);
        cmd.Parameters.AddWithValue("threshold", _opts.BloatThresholdPercent);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
            results.Add((reader.GetString(0), reader.GetDouble(1), reader.GetInt64(2)));

        return results;
    }
}
