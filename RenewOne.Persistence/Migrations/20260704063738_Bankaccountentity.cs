using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Bankaccountentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "Ledgers",
                newName: "CreditLimit");

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "QRMasters",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "AvailableBalance",
                table: "Ledgers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "BankAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IFSCCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpiId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountHolderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccount", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QRMasters_BankAccountId",
                table: "QRMasters",
                column: "BankAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_QRMasters_BankAccount_BankAccountId",
                table: "QRMasters",
                column: "BankAccountId",
                principalTable: "BankAccount",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QRMasters_BankAccount_BankAccountId",
                table: "QRMasters");

            migrationBuilder.DropTable(
                name: "BankAccount");

            migrationBuilder.DropIndex(
                name: "IX_QRMasters_BankAccountId",
                table: "QRMasters");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "QRMasters");

            migrationBuilder.DropColumn(
                name: "AvailableBalance",
                table: "Ledgers");

            migrationBuilder.RenameColumn(
                name: "CreditLimit",
                table: "Ledgers",
                newName: "Balance");
        }
    }
}
