using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEAS.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedFieldToBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Deleted",
                table: "Block",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Block");
        }
    }
}
