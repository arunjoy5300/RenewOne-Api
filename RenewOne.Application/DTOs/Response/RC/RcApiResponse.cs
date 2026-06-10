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

        [JsonPropertyName("owner_name")]
        public string OwnerName { get; set; } = string.Empty;

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_colour")]
        public string VehicleColour { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("chassis")]
        public string Chassis { get; set; } = string.Empty;

        [JsonPropertyName("engine")]
        public string Engine { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_manufacturer_name")]
        public string ManufacturerName { get; set; } = string.Empty;

        [JsonPropertyName("reg_date")]
        public string RegistrationDate { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_insurance_upto")]
        public string InsuranceValidity { get; set; } = string.Empty;
    }
}
