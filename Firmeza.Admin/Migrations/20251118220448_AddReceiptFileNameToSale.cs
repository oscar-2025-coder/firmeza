using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Firmeza.admi.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptFileNameToSale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptFileName",
                table: "Sales",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptFileName",
                table: "Sales");
        }
    }
}
