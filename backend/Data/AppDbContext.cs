// Infrastructure layer
using Microsoft.EntityFrameworkCore;
using ProductApi.Models;

namespace ProductApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<WeeklyAuditReportRow> WeeklyAuditReport => Set<WeeklyAuditReportRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        // Persist the role as its name ("Admin"/"Operator") rather than an int.
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        // Persist the report channel as its name ("None"/"Email"/"Sms").
        modelBuilder.Entity<User>()
            .Property(u => u.ReportChannel)
            .HasConversion<string>();

        // Partial unique index — AWB must be unique when set, nulls excluded
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Awb)
            .IsUnique()
            .HasFilter("\"Awb\" IS NOT NULL");

        // Optimistic concurrency: EF adds "WHERE Version = <old>" to every UPDATE/DELETE.
        // If the row changed since we read it, the write hits 0 rows and EF throws
        // instead of silently overwriting someone else's change.
        modelBuilder.Entity<Product>().Property(p => p.Version).IsConcurrencyToken();
        modelBuilder.Entity<User>().Property(u => u.Version).IsConcurrencyToken();
        modelBuilder.Entity<Order>().Property(o => o.Version).IsConcurrencyToken();

        // Email must be unique. Stored lower-cased (see UserService) so it's effectively
        // case-insensitive. Named separately from the GIN search index on Email — same
        // column, two indexes, exactly like Awb.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email, "IX_Users_Email_Unique")
            .IsUnique();

        // --- Substring search (pg_trgm) ---
        // The pg_trgm extension lets GIN indexes accelerate ILIKE '%term%' lookups.
        modelBuilder.HasPostgresExtension("pg_trgm");

        // pgstattuple measures index bloat for the maintenance/reindex job.
        modelBuilder.HasPostgresExtension("pgstattuple");

        // Read-only projection over the weekly audit metrics materialized view.
        // The view itself is created by raw SQL in the AddWeeklyAuditReportView
        // migration, so EF maps reads to it but never emits DDL for it.
        modelBuilder.Entity<WeeklyAuditReportRow>()
            .HasNoKey()
            .ToView("mv_weekly_audit_report");

        // Audit trail — JSONB snapshots of the row before/after each change.
        // Rows are inserted only by DB triggers (see the AddAuditTrail migration),
        // so the app treats this table as read-only.
        modelBuilder.Entity<AuditLog>(e =>
        {
            e.Property(a => a.OldData).HasColumnType("jsonb");
            e.Property(a => a.NewData).HasColumnType("jsonb");
            e.HasIndex(a => new { a.TableName, a.RowId });

            // Range index for the weekly report's "last week" filter. Without it the
            // report query Seq Scans all of AuditLogs and discards ~99% of rows; the
            // btree lets it walk straight to the date window.
            e.HasIndex(a => a.ChangedAt);
        });

        // GIN trigram indexes on every searchable column. These coexist with the
        // partial unique btree index on Awb above — different access methods.
        const string Gin = "gin";
        const string TrgmOps = "gin_trgm_ops";

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name)
            .HasMethod(Gin)
            .HasOperators(TrgmOps);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Name)
            .HasMethod(Gin)
            .HasOperators(TrgmOps);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasMethod(Gin)
            .HasOperators(TrgmOps);

        // Named overload — this is a SEPARATE index from the partial unique btree
        // index on Awb above. Without an explicit name EF treats both HasIndex calls
        // as one index and merges their config (unique + gin = invalid).
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Awb, "IX_Orders_Awb_Trgm")
            .HasMethod(Gin)
            .HasOperators(TrgmOps);
    }
}
