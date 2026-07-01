using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    RowId = table.Column<int>(type: "integer", nullable: false),
                    OldData = table.Column<string>(type: "jsonb", nullable: true),
                    NewData = table.Column<string>(type: "jsonb", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ChangedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableName_RowId",
                table: "AuditLogs",
                columns: new[] { "TableName", "RowId" });

            // Trigger function: snapshot the row before/after into AuditLogs
            // Fires for INSERT/UPDATE/DELETE; "who" comes from a per-transaction
            // session var the app sets (app.user_id) - null until Auth exists
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION write_audit()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    v_row_id int;
                BEGIN
                    IF TG_OP = 'DELETE' THEN
                        v_row_id := OLD."Id";
                    ELSE
                        v_row_id := NEW."Id";
                    END IF;

                    INSERT INTO "AuditLogs"
                        ("TableName", "Action", "RowId", "OldData", "NewData", "ChangedAt", "ChangedBy")
                    VALUES (
                        TG_TABLE_NAME,
                        TG_OP,
                        v_row_id,
                        CASE WHEN TG_OP = 'INSERT' THEN NULL ELSE to_jsonb(OLD) END,
                        CASE WHEN TG_OP = 'DELETE' THEN NULL ELSE to_jsonb(NEW) END,
                        now(),
                        NULLIF(current_setting('app.user_id', true), '')
                    );

                    RETURN NULL; -- AFTER trigger: return value is ignored
                END;
                $$;
                """);

            foreach (var table in new[] { "Orders", "Products", "Users" })
            {
                migrationBuilder.Sql($"""
                    CREATE TRIGGER "{table.ToLower()}_audit"
                    AFTER INSERT OR UPDATE OR DELETE ON "{table}"
                    FOR EACH ROW EXECUTE FUNCTION write_audit();
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in new[] { "Orders", "Products", "Users" })
                migrationBuilder.Sql($"DROP TRIGGER IF EXISTS \"{table.ToLower()}_audit\" ON \"{table}\";");

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS write_audit();");

            migrationBuilder.DropTable(
                name: "AuditLogs");
        }
    }
}
