using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnquiryQoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnquiryPricings");

            migrationBuilder.AlterColumn<decimal>(
                name: "AgentPoints",
                table: "PricingMasters",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SelectedQuoteId",
                table: "Enquiries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EnquiryQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsuranceCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompanyPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AgentPoints = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AdminProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Idv = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsManualQuote = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryQuotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnquiryQuotes_Enquiries_EnquiryId",
                        column: x => x.EnquiryId,
                        principalTable: "Enquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnquiryQuotes_InsuranceCompanies_InsuranceCompanyId",
                        column: x => x.InsuranceCompanyId,
                        principalTable: "InsuranceCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryQuotes_EnquiryId",
                table: "EnquiryQuotes",
                column: "EnquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryQuotes_InsuranceCompanyId",
                table: "EnquiryQuotes",
                column: "InsuranceCompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnquiryQuotes");

            migrationBuilder.DropColumn(
                name: "SelectedQuoteId",
                table: "Enquiries");

            migrationBuilder.AlterColumn<int>(
                name: "AgentPoints",
                table: "PricingMasters",
                type: "int",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "EnquiryPricings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdminProfit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AgentPoint = table.Column<int>(type: "int", nullable: false),
                    CompanyPayable = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPremium = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnquiryPricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnquiryPricings_Enquiries_EnquiryId",
                        column: x => x.EnquiryId,
                        principalTable: "Enquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryPricings_EnquiryId",
                table: "EnquiryPricings",
                column: "EnquiryId",
                unique: true);
        }
    }
}
