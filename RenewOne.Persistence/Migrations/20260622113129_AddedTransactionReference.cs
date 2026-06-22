using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedTransactionReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceNo",
                table: "WalletTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceNo",
                table: "WalletTransactions");
        }
    }
}
