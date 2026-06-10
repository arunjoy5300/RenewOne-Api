using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RenewOne.Application.DTOs.Request.RC;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Application.Interfaces.Services.RC;
using RenewOne.Infrastructure.Options;
using System.Net.Http.Json;

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

            if(request == null)
                _logger.LogError("RC Validation request is null for vehicle number: {VehicleNumber}", vehicleNumber);

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
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<RcApiResponse>(cancellationToken: cts.Token);

            // Validate API response
            if (apiResponse?.Data == null)
            {
                _logger.LogInformation("RC API response does not contain expected data for vehicle number: {VehicleNumber}", vehicleNumber);
            }

            if (apiResponse.Status?.Code != 200)
            {
                _logger.LogWarning(
                    "RC API returned non-success status. Code: {Code}, Type: {Type}, Message: {Message}",
                    apiResponse.Status?.Code,
                    apiResponse.Status?.Type,
                    apiResponse.Status?.Message);
            }

            var dto = MapResponse(apiResponse);

            _logger.LogInformation(
                "Successfully fetched RC details for vehicle: {VehicleNumber}. Status: {Status}, IsValid: {IsValid}",
                vehicleNumber,
                apiResponse.Status?.Code);

            return dto;

            return MapResponse(apiResponse);

        }
        private RcApiDto MapResponse(RcApiResponse response)
        {
            return new RcApiDto
            {
                VehicleNumber =
                    response.Data.RegNo,

                OwnerName =
                    response.Data.OwnerName,

                Model =
                    response.Data.Model,

                VehicleColor =
                    response.Data.VehicleColour,

                FuelType =
                    response.Data.Type,

                ChassisNumber =
                    response.Data.Chassis,

                EngineNumber =
                    response.Data.Engine,

                ManufacturerName =
                    response.Data.ManufacturerName,

                RegistrationDate = response.Data.RegistrationDate,

                InsuranceValidUpto =response.Data.InsuranceValidity
            };
        }
        private DateTime? ParseDate(string? date)
        {
            if (string.IsNullOrWhiteSpace(date))
                return null;

            if (DateTime.TryParseExact(
                date,
                "dd-MM-yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var parsedDate))
            {
                return parsedDate;
            }

            return null;
        }
    }
}