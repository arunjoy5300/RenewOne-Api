using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VendorIdAddedQuote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "EmpId",
                table: "Users",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "VendorId",
                table: "EnquiryQuotes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmpId",
                table: "Users",
                column: "EmpId",
                unique: true,
                filter: "[EmpId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EnquiryQuotes_VendorId",
                table: "EnquiryQuotes",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_EnquiryQuotes_Vendors_VendorId",
                table: "EnquiryQuotes",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnquiryQuotes_Vendors_VendorId",
                table: "EnquiryQuotes");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmpId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_EnquiryQuotes_VendorId",
                table: "EnquiryQuotes");

            migrationBuilder.DropColumn(
                name: "VendorId",
                table: "EnquiryQuotes");

            migrationBuilder.AlterColumn<string>(
                name: "EmpId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
