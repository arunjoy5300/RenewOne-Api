using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedTablesForQRPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTaskAllocations_InsuranceTypes_InsuranceTypeId",
                table: "UserTaskAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTaskAllocations_VehicleTypes_VehicleTypeId",
                table: "UserTaskAllocations");

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedByUserId",
                table: "EnquiryQuotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "EnquiryQuotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Ledgers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ledgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ledgers_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolicyPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnquiryQuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubmittedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VerifiedbyUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyPayments_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyPayments_Enquiries_EnquiryId",
                        column: x => x.EnquiryId,
                        principalTable: "Enquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyPayments_EnquiryQuotes_EnquiryQuoteId",
                        column: x => x.EnquiryQuoteId,
                        principalTable: "EnquiryQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyPayments_Users_SubmittedByUserId",
                        column: x => x.SubmittedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PolicyPayments_Users_VerifiedbyUserId",
                        column: x => x.VerifiedbyUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QRMasters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpiId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpiName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MerchantName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LedgerTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LedgerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EnquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LedgerTransactions_Ledgers_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "Ledgers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QRMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QRMasterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsuranceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehicleTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QRMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QRMappings_InsuranceTypes_InsuranceTypeId",
                        column: x => x.InsuranceTypeId,
                        principalTable: "InsuranceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QRMappings_QRMasters_QRMasterId",
                        column: x => x.QRMasterId,
                        principalTable: "QRMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QRMappings_VehicleTypes_VehicleTypeId",
                        column: x => x.VehicleTypeId,
                        principalTable: "VehicleTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryQuotes_UserId",
                table: "EnquiryQuotes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ledgers_AccountId",
                table: "Ledgers",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LedgerTransactions_LedgerId",
                table: "LedgerTransactions",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyPayments_AccountId",
                table: "PolicyPayments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyPayments_EnquiryId",
                table: "PolicyPayments",
                column: "EnquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyPayments_EnquiryQuoteId",
                table: "PolicyPayments",
                column: "EnquiryQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyPayments_SubmittedByUserId",
                table: "PolicyPayments",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyPayments_VerifiedbyUserId",
                table: "PolicyPayments",
                column: "VerifiedbyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QRMappings_InsuranceTypeId",
                table: "QRMappings",
                column: "InsuranceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_QRMappings_QRMasterId",
                table: "QRMappings",
                column: "QRMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_QRMappings_VehicleTypeId",
                table: "QRMappings",
                column: "VehicleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnquiryQuotes_Users_UserId",
                table: "EnquiryQuotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTaskAllocations_InsuranceTypes_InsuranceTypeId",
                table: "UserTaskAllocations",
                column: "InsuranceTypeId",
                principalTable: "InsuranceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTaskAllocations_VehicleTypes_VehicleTypeId",
                table: "UserTaskAllocations",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryQuotes_Users_UserId",
                table: "EnquiryQuotes");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTaskAllocations_InsuranceTypes_InsuranceTypeId",
                table: "UserTaskAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_UserTaskAllocations_VehicleTypes_VehicleTypeId",
                table: "UserTaskAllocations");

            migrationBuilder.DropTable(
                name: "LedgerTransactions");

            migrationBuilder.DropTable(
                name: "PolicyPayments");

            migrationBuilder.DropTable(
                name: "QRMappings");

            migrationBuilder.DropTable(
                name: "Ledgers");

            migrationBuilder.DropTable(
                name: "QRMasters");

            migrationBuilder.DropIndex(
                name: "IX_EnquiryQuotes_UserId",
                table: "EnquiryQuotes");

            migrationBuilder.DropColumn(
                name: "SelectedByUserId",
                table: "EnquiryQuotes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EnquiryQuotes");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTaskAllocations_InsuranceTypes_InsuranceTypeId",
                table: "UserTaskAllocations",
                column: "InsuranceTypeId",
                principalTable: "InsuranceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTaskAllocations_VehicleTypes_VehicleTypeId",
                table: "UserTaskAllocations",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id");
        }
    }
}
