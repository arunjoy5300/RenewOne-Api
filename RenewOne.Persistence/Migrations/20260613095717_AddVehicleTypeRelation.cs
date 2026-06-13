using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTypeRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleDetails_VehicleTypes_VehicleTypeId",
                table: "VehicleDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleEnquiryDetails_VehicleTypes_VehicleTypeId",
                table: "VehicleEnquiryDetails");

            migrationBuilder.DropIndex(
                name: "IX_VehicleDetails_VehicleTypeId",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "VehicleTypeId",
                table: "VehicleDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleTypeId",
                table: "VehicleEnquiryDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleEnquiryDetails_VehicleTypes_VehicleTypeId",
                table: "VehicleEnquiryDetails",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleEnquiryDetails_VehicleTypes_VehicleTypeId",
                table: "VehicleEnquiryDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleTypeId",
                table: "VehicleEnquiryDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "VehicleTypeId",
                table: "VehicleDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VehicleDetails_VehicleTypeId",
                table: "VehicleDetails",
                column: "VehicleTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleDetails_VehicleTypes_VehicleTypeId",
                table: "VehicleDetails",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleEnquiryDetails_VehicleTypes_VehicleTypeId",
                table: "VehicleEnquiryDetails",
                column: "VehicleTypeId",
                principalTable: "VehicleTypes",
                principalColumn: "Id");
        }
    }
}
