using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RenewOne.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialPricingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PricingMasters_InsuranceTypes_InsuranceTypeId",
                table: "PricingMasters");

            migrationBuilder.DropIndex(
                name: "IX_PricingMasters_InsuranceTypeId",
                table: "PricingMasters");

            migrationBuilder.DropColumn(
                name: "InsuranceTypeId",
                table: "PricingMasters");

            migrationBuilder.AddColumn<string>(
                name: "BodyType",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CylinderCount",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FinancerName",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GrossVehicleWeight",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InsuranceCompanyName",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicyNumber",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InsuranceValidUpto",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCommercial",
                table: "VehicleDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinanced",
                table: "VehicleDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManufacturerName",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ManufacturingMonthYear",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormsType",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerCount",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerFatherName",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PermanentAddress",
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
                name: "PresentAddress",
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

            migrationBuilder.AddColumn<string>(
                name: "PuccNumber",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PuccUpto",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RcExpiryDate",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationAuthority",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationDate",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RtoCode",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatCapacity",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnladenWeight",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleCategory",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleClass",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleColor",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleTaxUpto",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Wheelbase",
                table: "VehicleDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyType",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "CylinderCount",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "FinancerName",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "GrossVehicleWeight",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "InsuranceCompanyName",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "InsurancePolicyNumber",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "InsuranceValidUpto",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "IsCommercial",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "IsFinanced",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "ManufacturerName",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "ManufacturingMonthYear",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "NormsType",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "OwnerCount",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "OwnerFatherName",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PermanentAddress",
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
                name: "PresentAddress",
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

            migrationBuilder.DropColumn(
                name: "PuccNumber",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "PuccUpto",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "RcExpiryDate",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "RegistrationAuthority",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "RtoCode",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "SeatCapacity",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "UnladenWeight",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "VehicleCategory",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "VehicleClass",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "VehicleColor",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "VehicleTaxUpto",
                table: "VehicleDetails");

            migrationBuilder.DropColumn(
                name: "Wheelbase",
                table: "VehicleDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceTypeId",
                table: "PricingMasters",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PricingMasters_InsuranceTypeId",
                table: "PricingMasters",
                column: "InsuranceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PricingMasters_InsuranceTypes_InsuranceTypeId",
                table: "PricingMasters",
                column: "InsuranceTypeId",
                principalTable: "InsuranceTypes",
                principalColumn: "Id");
        }
    }
}
