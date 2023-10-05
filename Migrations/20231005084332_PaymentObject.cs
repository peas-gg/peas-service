using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEAS.Migrations
{
    /// <inheritdoc />
    public partial class PaymentObject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Payment_PaymentIntentId",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Payment_PaymentIntentId",
                table: "Orders");
        }
    }
}
