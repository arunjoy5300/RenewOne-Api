using Microsoft.Extensions.Configuration;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.Service;
using RenewOne.Application.Interfaces.Services.External.RC;
using RenewOne.Domain.Entities;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace RenewOne.Infrastructure.Services.External.RC
{
    public class RCIDSService : IRCIDSService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public RCIDSService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<Result<VehicleDetails>> GetVehicleDetailsAsync(string regNumber)
        {
            try
            {
                var requestBody = new 
                { 
                    api_id = _configuration["RcApiSettings:ApiId"], 
                    api_key = _configuration["RcApiSettings:ApiKey"], 
                    token_id = _configuration["RcApiSettings:TokenId"], 
                    reg_no = regNumber 
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/srv2/validation/rc", jsonContent);

                var responseString = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode) 
                { 
                    return Result<VehicleDetails>.Failure("Failed to fetch RC details", (int)response.StatusCode);
                } 
                
                // Deserialize JSON -> DTO
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var apiResponse = JsonSerializer.Deserialize<RcValidationResponseDto>( responseString, options);
                if (apiResponse == null) 
                { 
                    return Result<VehicleDetails>.Failure( "Invalid API response"); 
                } 

                // Business Failure
                if (apiResponse.Status.Code != 200) 
                { 
                    return Result<VehicleDetails>.Failure( apiResponse.Message, apiResponse.Status.Code);
                } 
                // Manual Mapping DTO -> Entity
                var vehicle = new VehicleDetails 
                { 
                    VehicleNumber = apiResponse.Data.Reg_No, 
                    VehicleClass = apiResponse.Data.Class,
                    ChassisNumber = apiResponse.Data.Chassis, 
                    EngineNumber = apiResponse.Data.Engine,
                    ManufacturerName = apiResponse.Data.Vehicle_Manufacturer_Name,
                    Model = apiResponse.Data.Model,
                    VehicleColor = apiResponse.Data.Vehicle_Colour,
                    FuelType = apiResponse.Data.Type,
                    NormsType = apiResponse.Data.Norms_Type,
                    BodyType = apiResponse.Data.Body_Type,
                    OwnerName = apiResponse.Data.Owner_Name,
                    RegistrationAuthority = apiResponse.Data.Reg_Authority,
                    RegistrationDate = apiResponse.Data.Reg_Date,
                    RcExpiryDate = apiResponse.Data.Rc_Expiry_Date,
                    InsuranceCompanyName = apiResponse.Data.Vehicle_Insurance_Company_Name,
                    InsuranceValidUpto = apiResponse.Data.Vehicle_Insurance_Upto,
                    CubicCapacity = apiResponse.Data.Vehicle_Cubic_Capacity, 
                    VehicleCategory = apiResponse.Data.Vehicle_Category,
                    SeatCapacity = apiResponse.Data.Vehicle_Seat_Capacity,
                    IsCommercial = apiResponse.Data.Is_Commercial,
                    RtoCode = apiResponse.Data.Rto_Code,
                    IsFinanced = apiResponse.Data.Financed,
                    RawApiResponse = responseString
                }; 
                return Result<VehicleDetails>.Success( vehicle, "Vehicle details fetched successfully");

            }
            catch(Exception ex)
            {
                return Result<VehicleDetails>.Failure("Something went wrong while fetching RC details", 500);
            }
        }
    }
}
