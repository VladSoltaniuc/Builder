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
    public DbSet<WeeklyAuditReportView> WeeklyAuditReport => Set<WeeklyAuditReportView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Product>()
            .Property(p => p.Category)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();

        modelBuilder.Entity<User>()
            .Property(u => u.ReportChannel)
            .HasConversion<string>();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Awb)
            .IsUnique()
            .HasFilter("\"Awb\" IS NOT NULL");

        modelBuilder.Entity<Product>().Property(p => p.Version).IsConcurrencyToken();
        modelBuilder.Entity<User>().Property(u => u.Version).IsConcurrencyToken();
        modelBuilder.Entity<Order>().Property(o => o.Version).IsConcurrencyToken();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email, "IX_Users_Email_Unique")
            .IsUnique();

        // The pg_trgm extension lets GIN indexes accelerate ILIKE '%term%' lookups
        modelBuilder.HasPostgresExtension("pg_trgm");

        // pgstattuple measures index bloat for the maintenance/reindex job
        modelBuilder.HasPostgresExtension("pgstattuple");

        modelBuilder.Entity<WeeklyAuditReportView>()
            .HasNoKey()
            .ToView("mv_weekly_audit_report");

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.Property(a => a.OldData).HasColumnType("jsonb");
            e.Property(a => a.NewData).HasColumnType("jsonb");
            e.HasIndex(a => new { a.TableName, a.RowId });
            e.HasIndex(a => a.ChangedAt);
        });

        // GIN trigram indexes coexist with the partial unique btree on Awb
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

        // Named overload, so EF doesn't treat both HasIndex calls as one index
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Awb, "IX_Orders_Awb_Trgm")
            .HasMethod(Gin)
            .HasOperators(TrgmOps);
    }
}
