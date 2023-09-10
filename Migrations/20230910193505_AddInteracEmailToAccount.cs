using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEAS.Migrations
{
    /// <inheritdoc />
    public partial class AddInteracEmailToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InteracEmail",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InteracEmail",
                table: "Accounts");
        }
    }
}
