using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedNullForWalletTnx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_PaymentOrders_PaymentOrderId",
                table: "WalletTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentOrderId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WalletTransactions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_PaymentOrders_PaymentOrderId",
                table: "WalletTransactions",
                column: "PaymentOrderId",
                principalTable: "PaymentOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_PaymentOrders_PaymentOrderId",
                table: "WalletTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaymentOrderId",
                table: "WalletTransactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WalletTransactions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_PaymentOrders_PaymentOrderId",
                table: "WalletTransactions",
                column: "PaymentOrderId",
                principalTable: "PaymentOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
