using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyAuditReportView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Weekly audit metrics, one row per audited table, counting last week's
            // changes by action. The VALUES list anchors all three tables so each
            // always appears - the LEFT JOIN yields zero counts for a quiet week
            //
            // "Last week" = the previous full Mon–Sun: from the Monday before this
            // one up to (but not including) this week's Monday. date_trunc('week')
            // returns Monday 00:00, so it's stable no matter when the view refreshes
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

            // Unique index on the single key column: required for REFRESH ...
            // CONCURRENTLY and keeps lookups cheap
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX ix_mv_weekly_audit_report_table
                ON mv_weekly_audit_report ("TableName");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP MATERIALIZED VIEW IF EXISTS mv_weekly_audit_report;");
        }
    }
}
