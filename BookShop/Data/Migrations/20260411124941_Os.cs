using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class Os : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_OrderStatus_OrderStatusid",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "OrderStatusid",
                table: "Order",
                newName: "OrderStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Order_OrderStatusid",
                table: "Order",
                newName: "IX_Order_OrderStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_OrderStatus_OrderStatusId",
                table: "Order",
                column: "OrderStatusId",
                principalTable: "OrderStatus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_OrderStatus_OrderStatusId",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "OrderStatusId",
                table: "Order",
                newName: "OrderStatusid");

            migrationBuilder.RenameIndex(
                name: "IX_Order_OrderStatusId",
                table: "Order",
                newName: "IX_Order_OrderStatusid");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_OrderStatus_OrderStatusid",
                table: "Order",
                column: "OrderStatusid",
                principalTable: "OrderStatus",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
