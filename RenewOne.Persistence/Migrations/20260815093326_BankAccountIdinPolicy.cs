using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BankAccountIdinPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "PolicyPayments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyPayments_BankAccountId",
                table: "PolicyPayments",
                column: "BankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_PolicyPayments_BankAccount_BankAccountId",
                table: "PolicyPayments",
                column: "BankAccountId",
                principalTable: "BankAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PolicyPayments_BankAccount_BankAccountId",
                table: "PolicyPayments");

            migrationBuilder.DropIndex(
                name: "IX_PolicyPayments_BankAccountId",
                table: "PolicyPayments");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "PolicyPayments");
        }
    }
}
