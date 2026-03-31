using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XPos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAvailableToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Staffs",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Products",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "Staffs");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Products");
        }
    }
}
