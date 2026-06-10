using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Services.RC;
using RenewOne.Domain.Entities;
using System.Text.Json;

namespace RenewOne.Application.LogicService.Enquiry.VehicleEnquiry
{
    public class VehicleEnquiryLogicService : IVehicleEnquiryLogicService
    {
        private readonly IVehicleEnquiryRepository _vehicleEnquiryRepository;
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly IRCService _rcApiService;
        public VehicleEnquiryLogicService(
            IVehicleEnquiryRepository vehicleEnquiryRepository, 
            IRCService rcApiService,
            IEnquiryRepository enquiryRepository)
        {
            _vehicleEnquiryRepository = vehicleEnquiryRepository;
            _rcApiService = rcApiService;
            _enquiryRepository = enquiryRepository;
        }
        public async Task<Result<bool>> CreateVehicleEnquiryAsync(CreateVehicleEnquiry request, Guid tenantId, Guid userId)
        {
            var formattedNumber = request.VehicleNumber.ToUpper().Replace(" ", "");

            var existingEnquiry = _enquiryRepository.GetEnquiries().Any(x =>
                x.VehicleEnquiryDetails != null &&
                x.VehicleEnquiryDetails.VehicleDetails != null &&
                x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber == formattedNumber &&
                x.Status != "Completed" &&
                x.CreatedAt >= DateTime.UtcNow.AddDays(-180));

            if(existingEnquiry)
            {
                return Result<bool>.Conflict("An enquiry for this vehicle already exists.");
            }

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
                var rcApiResponse = await _rcApiService.FetchVehicleDetailsAsync(formattedNumber);

                if (rcApiResponse == null)
                {
                    return Result<bool>.Failure("Unable to fetch vehicle details from RC API.Please try submitting manually", 400);
                }

                vehicle = new VehicleDetails
                {
                    Id = Guid.NewGuid(),
                    VehicleNumber = formattedNumber,
                    VehicleTypeId = request.VehicleTypeId,
                    ChassisNumber = rcApiResponse.ChassisNumber,
                    EngineNumber = rcApiResponse.EngineNumber,
                    MakerModel = rcApiResponse.MakerModel,
                    FuelType = rcApiResponse.FuelType,
                    CubicCapacity = rcApiResponse.CubicCapacity,
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
                return Result<bool>.Failure("Failed to create vehicle enquiry.", 500);

            return Result<bool>.Success(true, "Vehicle enquiry successfully registered.", 201);
        }
    }
}
