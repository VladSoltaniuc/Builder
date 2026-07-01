using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class RenameReadOnlyRoleToOperator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The "ReadOnly" role was renamed to "Operator" - migrate existing rows and
            // the column default to match the new enum name
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'Operator' WHERE \"Role\" = 'ReadOnly';");
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"Role\" SET DEFAULT 'Operator';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE \"Users\" ALTER COLUMN \"Role\" SET DEFAULT 'ReadOnly';");
            migrationBuilder.Sql("UPDATE \"Users\" SET \"Role\" = 'ReadOnly' WHERE \"Role\" = 'Operator';");
        }
    }
}
