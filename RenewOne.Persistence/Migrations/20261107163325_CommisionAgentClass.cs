using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CommisionAgentClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreditLimit",
                table: "Ledgers");

            migrationBuilder.AddColumn<bool>(
                name: "AgentClassCommissionApplied",
                table: "EnquiryQuotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "AgentClassCommissionDeduction",
                table: "EnquiryQuotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AgentClassCommissionPercentage",
                table: "EnquiryQuotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440000"),
                column: "CommSharePercentage",
                value: 5m);

            migrationBuilder.UpdateData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440001"),
                column: "CommSharePercentage",
                value: 15m);

            migrationBuilder.UpdateData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440002"),
                column: "CommSharePercentage",
                value: 15m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentClassCommissionApplied",
                table: "EnquiryQuotes");

            migrationBuilder.DropColumn(
                name: "AgentClassCommissionDeduction",
                table: "EnquiryQuotes");

            migrationBuilder.DropColumn(
                name: "AgentClassCommissionPercentage",
                table: "EnquiryQuotes");

            migrationBuilder.AddColumn<decimal>(
                name: "CreditLimit",
                table: "Ledgers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440000"),
                column: "CommSharePercentage",
                value: 15m);

            migrationBuilder.UpdateData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440001"),
                column: "CommSharePercentage",
                value: 10m);

            migrationBuilder.UpdateData(
                table: "AgentClasses",
                keyColumn: "Id",
                keyValue: new Guid("550e8400-e29b-41d4-a716-446655440002"),
                column: "CommSharePercentage",
                value: 5m);
        }
    }
}
