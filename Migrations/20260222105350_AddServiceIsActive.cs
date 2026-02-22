using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartWash.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceIsActive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ServicePrices",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ServicePrices");
        }
    }
}
