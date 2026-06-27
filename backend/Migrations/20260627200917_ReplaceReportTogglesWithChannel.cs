using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceReportTogglesWithChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeeklyReportSmsSubscribed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WeeklyReportSubscribed",
                table: "Users");

            // Existing users default to not subscribed.
            migrationBuilder.AddColumn<string>(
                name: "ReportChannel",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "None");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportChannel",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "WeeklyReportSmsSubscribed",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WeeklyReportSubscribed",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
