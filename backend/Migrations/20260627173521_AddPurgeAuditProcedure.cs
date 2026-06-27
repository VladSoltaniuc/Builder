using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPurgeAuditProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A PROCEDURE (not a function) because it COMMITs between batches —
            // deleting in 10k chunks avoids one huge lock/transaction on a big table.
            // Only a procedure may issue COMMIT; a function runs in a single transaction.
            migrationBuilder.Sql("""
                CREATE PROCEDURE purge_audit(older_than_days int)
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    v_deleted int;
                BEGIN
                    LOOP
                        DELETE FROM "AuditLogs"
                        WHERE "Id" IN (
                            SELECT "Id" FROM "AuditLogs"
                            WHERE "ChangedAt" < now() - make_interval(days => older_than_days)
                            LIMIT 10000
                        );

                        GET DIAGNOSTICS v_deleted = ROW_COUNT;
                        EXIT WHEN v_deleted = 0;
                        COMMIT;
                    END LOOP;
                END;
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS purge_audit(int);");
        }
    }
}
