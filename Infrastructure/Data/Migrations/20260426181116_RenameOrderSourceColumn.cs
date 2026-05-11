using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventorySalesManagementSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameOrderSourceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderSorce",
                table: "Sales",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "OrderSource",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderSorce",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "OrderSource",
                table: "Orders");
        }
    }
}
