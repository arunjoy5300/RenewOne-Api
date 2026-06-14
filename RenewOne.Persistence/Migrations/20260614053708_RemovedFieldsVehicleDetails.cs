using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovedFieldsVehicleDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MakerModel",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PermanentCity",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PermanentDistrict",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PermanentPincode",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PermanentState",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PresentCity",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PresentDistrict",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PresentPincode",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PresentState",
                table: "VehicleDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleDetailsId",
                table: "VehicleEnquiryDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "VehicleDetailsId",
                table: "VehicleEnquiryDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MakerModel",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PermanentCity",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PermanentDistrict",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PermanentPincode",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PermanentState",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PresentCity",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PresentDistrict",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PresentPincode",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PresentState",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
