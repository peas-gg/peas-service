using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEAS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCurrencyFromPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payment_Currency",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Payment_Currency",
                table: "Orders",
                type: "int",
                nullable: true);
        }
    }
}
