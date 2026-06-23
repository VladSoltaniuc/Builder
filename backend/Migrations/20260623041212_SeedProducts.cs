using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: ["Name", "Category", "Price", "Stock", "Version"],
                values: new object[,]
                {
                    { "Mechanical Keyboard", "Peripherals",  349.99m, 25, 1 },
                    { "Wireless Mouse",      "Peripherals",  149.50m, 60, 1 },
                    { "27\" 144Hz Monitor",  "Monitors",    1299.00m, 12, 1 },
                    { "NVMe SSD 1TB",        "Storage",      459.00m, 40, 1 },
                    { "Gaming Headset",      "Audio",        279.99m, 18, 1 },
                    { "USB-C Hub 7-in-1",    "Accessories",   89.99m, 75, 1 },
                    { "Webcam 1080p",        "Peripherals",  199.00m, 30, 1 },
                    { "Standing Desk",       "Furniture",    899.00m,  8, 1 },
                    { "Monitor Arm",         "Accessories",  149.00m, 22, 1 },
                    { "LED Desk Lamp",       "Lighting",      59.99m, 50, 1 },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"Products\" WHERE \"Name\" IN ('Mechanical Keyboard','Wireless Mouse','27\" 144Hz Monitor','NVMe SSD 1TB','Gaming Headset','USB-C Hub 7-in-1','Webcam 1080p','Standing Desk','Monitor Arm','LED Desk Lamp')");
        }
    }
}
