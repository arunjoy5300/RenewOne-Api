using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RenewOne.Application.DTOs.Response.RC
{
    public class RcApiResponse
    {
        [JsonPropertyName("status")]
        public StatusInfo Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public RcApiData Data { get; set; }
    }

    public class StatusInfo
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
    public class RcApiData
    {
        [JsonPropertyName("reg_no")]
        public string RegNo { get; set; } = string.Empty;

        [JsonPropertyName("class")]
        public string Class { get; set; } = string.Empty;

        [JsonPropertyName("chassis")]
        public string Chassis { get; set; } = string.Empty;

        [JsonPropertyName("engine")]
        public string Engine { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_manufacturer_name")]
        public string ManufacturerName { get; set; } = string.Empty;

        [JsonPropertyName("owner_name")]
        public string OwnerName { get; set; } = string.Empty;

        [JsonPropertyName("owner_father_name")]
        public string OwnerFatherName { get; set; } = string.Empty;

        [JsonPropertyName("mobile_number")]
        public string MobileNumber { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_colour")]
        public string VehicleColour { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("norms_type")]
        public string NormsType { get; set; } = string.Empty;

        [JsonPropertyName("owner_count")]
        public string OwnerCount { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("reg_date")]
        public string RegistrationDate { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_insurance_upto")]
        public string InsuranceValidity { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_cubic_capacity")]
        public string CubicCapacity { get; set; } = string.Empty;

        [JsonPropertyName("present_address")]
        public string PresentAddress { get; set; } = string.Empty;

        [JsonPropertyName("permanent_address")]
        public string PermanentAddress { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_category")]
        public string VehicleCategory { get; set; } = string.Empty;

        [JsonPropertyName("pucc_number")]
        public string PuccNumber { get; set; } = string.Empty;

        [JsonPropertyName("pucc_upto")]
        public string PuccUpto { get; set; } = string.Empty;

        [JsonPropertyName("rc_expiry_date")]
        public string RcExpiryDate { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_tax_upto")]
        public string Taxupto { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_insurance_company_name")]
        public string VehicleInsuranceCompany { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_insurance_policy_number")]
        public string VehiclePolicyNumber { get; set; } = string.Empty;

        [JsonPropertyName("financed")]
        public bool Financed { get; set; }

        [JsonPropertyName("rc_financer")]
        public string RcFinancer { get; set; } = string.Empty;

        [JsonPropertyName("reg_authority")]
        public string RegAuthority { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_manufacturing_month_year")]
        public string ManufacturingMonthYear { get; set; } = string.Empty;

        [JsonPropertyName("is_commercial")]
        public string IsCommercial { get; set; }

        [JsonPropertyName("vehicle_cylinders_no")]
        public string VehicleCylinderNumber { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_seat_capacity")]
        public string VehicleSeatCapacity { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_standing_capacity")]
        public string VehicleStandingCapacity { get; set; } = string.Empty;

        [JsonPropertyName("gross_vehicle_weight")]
        public string GrossVehicleWeight { get; set; } = string.Empty;
    }
}
