using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWash.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByIdToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "LaundryOrders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "LaundryOrders");
        }
    }
}
