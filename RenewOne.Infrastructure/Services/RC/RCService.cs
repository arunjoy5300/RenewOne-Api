using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RenewOne.Application.Common.Helper;
using RenewOne.Application.DTOs.Request.RC;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Application.Interfaces.Services.RC;
using RenewOne.Infrastructure.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace RenewOne.Infrastructure.Services.RC
{
    public class RCService : IRCService
    {
        private readonly RcServiceOptions _options;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<RCService> _logger;
        public RCService(
            HttpClient httpClient, 
            IConfiguration configuration, 
            IOptions<RcServiceOptions> options,
            ILogger<RCService> logger)
        {
            _configuration = configuration;
            _options = options.Value;
            _httpClient = httpClient;
            _logger = logger;
        }
        public async Task<RcApiDto> FetchVehicleDetailsAsync(string vehicleNumber, CancellationToken cancellationToken = default)
        {
            var request = new RcValidationRequest
            {
                ApiId = _options.ApiId,
                ApiKey = _options.ApiKey,
                TokenId = _options.TokenId,
                RegNo = vehicleNumber
            };

            var url =
                $"{_options.BaseUrl}/srv2/validation/rc";

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var response = await _httpClient.PostAsJsonAsync(
                url,
                request,
                cancellationToken: cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cts.Token);
                _logger.LogError(
                    "RC API call failed with status {StatusCode}: {ErrorContent}",
                    response.StatusCode,
                    errorContent);
                return null;
            }

            var rawJson = await response.Content.ReadAsStringAsync(cts.Token);
            var apiResponse = JsonSerializer.Deserialize<RcApiResponse>(rawJson);

            // Validate API response
            if (apiResponse?.Data == null)
            {
                _logger.LogInformation("RC API response does not contain expected data for vehicle number: {VehicleNumber}", vehicleNumber);
                return null;
            }

            if (apiResponse?.Status?.Code != 200)
            {
                _logger.LogWarning(
                    "RC API returned non-success status. Code: {Code}, Type: {Type}, Message: {Message}",
                    apiResponse?.Status?.Code,
                    apiResponse?.Status?.Type,
                    apiResponse?.Status?.Message);
                return null;
            }

            var dto = MapResponse(apiResponse,rawJson);

            _logger.LogInformation(
                "Successfully fetched RC details for vehicle: {VehicleNumber}. Status: {Status}",
                vehicleNumber,
                apiResponse?.Status?.Code);

            return dto;
        }
        private RcApiDto MapResponse(RcApiResponse response, string rawJson)
        {
            if(response?.Data == null)
            {
                _logger.LogWarning("Attempted to map RC API response, but Data is null.");
                return null;
            }

            return new RcApiDto
            {
                VehicleNumber = response.Data.RegNo,
                VehicleClass = response.Data.Class,
                OwnerName = response.Data.OwnerName,
                MobileNumber = response.Data.MobileNumber,
                Model = response.Data.Model,
                VehicleColor = response.Data.VehicleColour,
                FuelType = response.Data.Type,
                NormsType = response.Data.NormsType,
                PresentAddress = response.Data.PresentAddress,
                PermanentAddress = response.Data.PermanentAddress,
                ChassisNumber = response.Data.Chassis,
                EngineNumber = response.Data.Engine,
                ManufacturerName = response.Data.ManufacturerName,
                RegistrationDate = DateHelper.ParseDateOnly(response.Data.RegistrationDate),
                InsuranceValidUpto = DateHelper.ParseDateOnly(response.Data.InsuranceValidity),
                InsuranceCompanyName = response.Data.VehicleInsuranceCompany,
                InsurancePolicyNumber = response.Data.VehiclePolicyNumber,
                FinancerName = response.Data.RcFinancer,
                IsFinanced = response.Data.Financed,
                VehicleTaxUpto = DateHelper.ParseDateOnly(response.Data.Taxupto),
                RcExpiryDate = DateHelper.ParseDateOnly(response.Data.RcExpiryDate),
                PuccNumber = response.Data.PuccNumber,
                PuccUpto = DateHelper.ParseDateOnly(response.Data.PuccUpto),
                VehicleCategory = response.Data.VehicleCategory,
                OwnerCount = response.Data.OwnerCount,
                OwnerFatherName = response.Data.OwnerFatherName,
                RegistrationAuthority = response.Data.RegAuthority,
                ManufacturingMonthYear = response.Data.ManufacturingMonthYear,
                IsCommercial = response.Data.IsCommercial,
                GrossVehicleWeight = response.Data.GrossVehicleWeight,
                CylinderCount = response.Data.VehicleCylinderNumber,
                SeatCapacity = response.Data.VehicleSeatCapacity,
                RawApiResponse = rawJson
            };
        }
    }
}