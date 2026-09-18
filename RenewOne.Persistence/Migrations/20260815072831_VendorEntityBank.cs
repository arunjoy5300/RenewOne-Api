using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VendorEntityBank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountName",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "IFSC",
                table: "Vendors");

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "Vendors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_BankAccountId",
                table: "Vendors",
                column: "BankAccountId",
                unique: true,
                filter: "[BankAccountId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendors_BankAccount_BankAccountId",
                table: "Vendors",
                column: "BankAccountId",
                principalTable: "BankAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendors_BankAccount_BankAccountId",
                table: "Vendors");

            migrationBuilder.DropIndex(
                name: "IX_Vendors_BankAccountId",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "Vendors");

            migrationBuilder.AddColumn<string>(
                name: "AccountName",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IFSC",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
