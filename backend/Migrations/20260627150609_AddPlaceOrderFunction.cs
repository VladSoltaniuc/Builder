using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceOrderFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION place_order(
                    p_user_id    int,
                    p_product_id int,
                    p_quantity   int)
                RETURNS int
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    v_price    numeric;
                    v_stock    int;
                    v_order_id int;
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM "Users" WHERE "Id" = p_user_id) THEN
                        RAISE EXCEPTION 'USER_NOT_FOUND';
                    END IF;

                    -- Lock the product row for the rest of this transaction so two
                    -- concurrent orders can't both read the same stock and oversell.
                    SELECT "Price", "Stock" INTO v_price, v_stock
                    FROM "Products"
                    WHERE "Id" = p_product_id
                    FOR UPDATE;

                    IF NOT FOUND THEN
                        RAISE EXCEPTION 'PRODUCT_NOT_FOUND';
                    END IF;

                    IF v_stock < p_quantity THEN
                        RAISE EXCEPTION 'INSUFFICIENT_STOCK';
                    END IF;

                    UPDATE "Products"
                    SET "Stock" = "Stock" - p_quantity
                    WHERE "Id" = p_product_id;

                    INSERT INTO "Orders"
                        ("UserId", "ProductId", "Quantity", "TotalPrice", "Status", "CreatedAt", "Version")
                    VALUES
                        (p_user_id, p_product_id, p_quantity, v_price * p_quantity, 'Pending', now(), 1)
                    RETURNING "Id" INTO v_order_id;

                    RETURN v_order_id;
                END;
                $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS place_order(int, int, int);");
        }
    }
}
