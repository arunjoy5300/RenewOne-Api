using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnquiryNumberInsertion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnquiryNumber",
                table: "Enquiries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceProductDisplays_InsuranceTypeId",
                table: "InsuranceProductDisplays",
                column: "InsuranceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InsuranceProductDisplays_InsuranceTypes_InsuranceTypeId",
                table: "InsuranceProductDisplays",
                column: "InsuranceTypeId",
                principalTable: "InsuranceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InsuranceProductDisplays_InsuranceTypes_InsuranceTypeId",
                table: "InsuranceProductDisplays");

            migrationBuilder.DropIndex(
                name: "IX_InsuranceProductDisplays_InsuranceTypeId",
                table: "InsuranceProductDisplays");

            migrationBuilder.DropColumn(
                name: "EnquiryNumber",
                table: "Enquiries");
        }
    }
}
