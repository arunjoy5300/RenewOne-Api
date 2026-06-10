using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Services.RC;
using RenewOne.Domain.Entities;
using System.Text.Json;

namespace RenewOne.Application.LogicService.Enquiry.VehicleEnquiry
{
    public class VehicleEnquiryLogicService : IVehicleEnquiryLogicService
    {
        private readonly IVehicleEnquiryRepository _vehicleEnquiryRepository;
        private readonly IRCService _rcApiService;
        public VehicleEnquiryLogicService(IVehicleEnquiryRepository vehicleEnquiryRepository, IRCService rcApiService)
        {
            _vehicleEnquiryRepository = vehicleEnquiryRepository;
            _rcApiService = rcApiService;
        }
        public async Task<Result<bool>> CreateVehicleEnquiryAsync(CreateVehicleEnquiry request, Guid tenantId, Guid userId)
        {
            var formattedNumber = request.VehicleNumber.ToUpper().Replace(" ", "");

            var enquiry = new Domain.Entities.Enquiry
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                CustomerName = request.CustomerName,
                CustomerNumber = request.CustomerNumber,
                InsuranceTypeId = request.InsuranceTypeId,
                CreatedByUserId = userId,
                Priority = request.Priority,
                Status = "Created",
                AssignedEmployeeId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await _vehicleEnquiryRepository.AddEnquiryAsync(enquiry);

            var vehicle = await _vehicleEnquiryRepository.GetVehicleDetailsByNumberAsync(formattedNumber);
            string rawApiResponseString = null;

            if (vehicle == null)
            {
                var rcApiJsonString = await _rcApiService.FetchVehicleDetailsAsync(formattedNumber);

                if (string.IsNullOrWhiteSpace(rcApiJsonString))
                {
                    return Result<bool>.Failure("API വഴി വണ്ടിയുടെ വിവരങ്ങൾ എടുക്കാൻ സാധിച്ചില്ല. ദയവായി മാനുവൽ എൻട്രി ഉപയോഗിക്കുക.", 400);
                }

                rawApiResponseString = rcApiJsonString;

                var apiData = JsonSerializer.Deserialize<RcApiDto>(rcApiJsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (apiData == null)
                {
                    return Result<bool>.Failure("API ഡാറ്റ വായിക്കാൻ സാധിച്ചില്ല.", 400);
                }

                vehicle = new VehicleDetails
                {
                    Id = Guid.NewGuid(),
                    VehicleNumber = formattedNumber,
                    VehicleTypeId = request.VehicleTypeId,
                    ChassisNumber = apiData.ChassisNumber,
                    EngineNumber = apiData.EngineNumber,
                    MakerModel = apiData.MakerModel,
                    FuelType = apiData.FuelType,
                    CubicCapacity = apiData.CubicCapacity,
                    CreatedAt = DateTime.UtcNow,
                    RawApiResponse = rawApiResponseString
                };

                await _vehicleEnquiryRepository.AddVehicleDetailsAsync(vehicle);
            }

            var enquiryDetails = new VehicleEnquiryDetails
            {
                Id = Guid.NewGuid(),
                EnquiryId = enquiry.Id,
                VehicleDetailsId = vehicle.Id,
                IdvValue = null,
                IsRcOwnerChanged = request.IsRCOwnerChanged,
                HasPreviousClaim = request.HasPreviousClaim,
                NcbPercentage = request.NcbPercentage,
                CreatedAt = DateTime.UtcNow
            };
            await _vehicleEnquiryRepository.AddVehicleEnquiryDetailsAsync(enquiryDetails);

            var success = await _vehicleEnquiryRepository.SaveChangesAsync();

            if (!success)
                return Result<bool>.Failure("എൻക്വയറി സേവ് ചെയ്യാൻ സാധിച്ചില്ല.", 500);

            return Result<bool>.Success(true, "വെഹിക്കിൾ എൻക്വയറി വിജയകരമായി രജിസ്റ്റർ ചെയ്തു.", 201);

        }
    }
}
