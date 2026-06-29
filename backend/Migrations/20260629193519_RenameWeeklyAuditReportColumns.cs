using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameWeeklyAuditReportColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Recreate the view with the action counts named for what they are
            // (Inserts/Updates/Deletes) instead of Created/Updated/Deleted.
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_weekly_audit_report;");

            migrationBuilder.Sql("""
                CREATE MATERIALIZED VIEW mv_weekly_audit_report AS
                SELECT
                    t.table_name                                              AS "TableName",
                    COUNT(a."Id") FILTER (WHERE a."Action" = 'INSERT')        AS "Inserts",
                    COUNT(a."Id") FILTER (WHERE a."Action" = 'UPDATE')        AS "Updates",
                    COUNT(a."Id") FILTER (WHERE a."Action" = 'DELETE')        AS "Deletes"
                FROM (VALUES ('Products'), ('Users'), ('Orders')) AS t(table_name)
                LEFT JOIN "AuditLogs" a
                    ON a."TableName" = t.table_name
                   AND a."ChangedAt" >= date_trunc('week', now()) - interval '7 days'
                   AND a."ChangedAt" <  date_trunc('week', now())
                GROUP BY t.table_name
                WITH DATA;
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ix_mv_weekly_audit_report_table
                ON mv_weekly_audit_report ("TableName");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_weekly_audit_report;");

            migrationBuilder.Sql("""
                CREATE MATERIALIZED VIEW mv_weekly_audit_report AS
                SELECT
                    t.table_name                                              AS "TableName",
                    COUNT(a."Id") FILTER (WHERE a."Action" = 'INSERT')        AS "Created",
                    COUNT(a."Id") FILTER (WHERE a."Action" = 'UPDATE')        AS "Updated",
                    COUNT(a."Id") FILTER (WHERE a."Action" = 'DELETE')        AS "Deleted"
                FROM (VALUES ('Products'), ('Users'), ('Orders')) AS t(table_name)
                LEFT JOIN "AuditLogs" a
                    ON a."TableName" = t.table_name
                   AND a."ChangedAt" >= date_trunc('week', now()) - interval '7 days'
                   AND a."ChangedAt" <  date_trunc('week', now())
                GROUP BY t.table_name
                WITH DATA;
                """);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ix_mv_weekly_audit_report_table
                ON mv_weekly_audit_report ("TableName");
                """);
        }
    }
}
