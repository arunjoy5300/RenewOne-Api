using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Services.External.RC;
using RenewOne.Domain.Entities;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.Enquiry.VehicleEnquiry
{
    public class VehicleEnquiryLogicService : IVehicleEnquiryLogicService
    {
        private readonly IVehicleEnquiryRepository _vehicleEnquiryRepository;
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly IRCIDSService _rcApiService;
        private readonly ILogger<IVehicleEnquiryLogicService> _logger;
        public VehicleEnquiryLogicService(
            IVehicleEnquiryRepository vehicleEnquiryRepository, 
            IRCIDSService rcApiService,
            IEnquiryRepository enquiryRepository,
            ILogger<IVehicleEnquiryLogicService> logger)
        {
            _vehicleEnquiryRepository = vehicleEnquiryRepository;
            _rcApiService = rcApiService;
            _enquiryRepository = enquiryRepository;
            _logger = logger;
        }
        public async Task<Result<VehicleEnquiryResponse>> CreateVehicleEnquiryAsync(CreateVehicleEnquiry request, Guid tenantId, Guid userId)
        {
            var vehicleNumber = request.VehicleNumber?.ToUpper().Replace(" ", "");

            _logger.LogInformation("Enquiry received for vehicle : {VehicleNumber}", vehicleNumber);

            var existingEnquiry = _enquiryRepository.GetEnquiries().Any(x =>
                x.VehicleEnquiryDetails != null &&
                x.VehicleEnquiryDetails.VehicleDetails != null &&
                x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber == vehicleNumber &&
                x.Status != "Completed" &&
                x.CreatedAt >= DateTime.UtcNow.AddDays(-180));

            if (existingEnquiry)
                return Result<VehicleEnquiryResponse>.Conflict(
                    "An enquiry for this vehicle already exists.");

            // Step 1 : Check local database first
            var vehicle = await _vehicleEnquiryRepository
                .GetVehicleDetailsByNumberAsync(vehicleNumber);

            // Step 2 : Vehicle not found locally
            if (vehicle == null)
            {
                if (request.SubmissionMode == SubmissionMode.Api)
                {
                    _logger.LogInformation(
                        "Vehicle not found locally. Fetching from RC API");

                    var rcData = await _rcApiService
                        .FetchVehicleDetailsAsync(vehicleNumber);

                    if (rcData == null)
                    {
                        return Result<VehicleEnquiryResponse>.Failure(
                            "Unable to fetch vehicle details from RC API. Please submit manually.",
                            400);
                    }

                    vehicle = MapVehicleDetails(rcData, vehicleNumber, request.CustomerNumber);

                    await _vehicleEnquiryRepository
                        .AddVehicleDetailsAsync(vehicle);
                }
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _vehicleEnquiryRepository.AddEnquiryAsync(enquiry);

            // Step 4 : Create Vehicle Enquiry Details
            var enquiryDetails = new VehicleEnquiryDetails
            {
                Id = Guid.NewGuid(),
                EnquiryId = enquiry.Id,
                VehicleDetailsId = vehicle?.Id,
                VehicleTypeId = request.VehicleTypeId,
                IdvValue = null,
                MIPolicyType = request.MIPolicyType,
                IsRcOwnerChanged = request.IsRCOwnerChanged,
                HasPreviousClaim = request.HasPreviousClaim,
                NcbPercentage = request.NcbPercentage,
                CreatedAt = DateTime.UtcNow
            };

            await _vehicleEnquiryRepository
                .AddVehicleEnquiryDetailsAsync(enquiryDetails);

            var success = await _vehicleEnquiryRepository.SaveChangesAsync();

            if (!success)
            {
                return Result<VehicleEnquiryResponse>.Failure(
                    "Failed to create vehicle enquiry.",
                    500);
            }

            var response = new VehicleEnquiryResponse
            {
                EnquiryId = enquiry.Id,
                Brand = vehicle?.ManufacturerName,
                Model = vehicle?.Model,
                CubicCapacity = vehicle?.CubicCapacity,
                ManufactureYear = vehicle?.ManufacturingMonthYear,
                FuelType = vehicle?.FuelType,
                SubmissionType = request.MIPolicyType
            };

            return Result<VehicleEnquiryResponse>.Success(
                response,
                "Vehicle enquiry successfully registered.");
        }

        private static VehicleDetails MapVehicleDetails(RcApiDto rc,string vehicleNumber,string phoneNumber)
        {
            return new VehicleDetails
            {
                Id = Guid.NewGuid(),
                VehicleNumber = vehicleNumber,
                OwnerName = rc.OwnerName,
                OwnerFatherName = rc.OwnerFatherName,
                MobileNumber = string.IsNullOrWhiteSpace(rc.MobileNumber) ? phoneNumber : rc.MobileNumber,
                InsuranceCompanyName = rc.InsuranceCompanyName,
                InsurancePolicyNumber = rc.InsurancePolicyNumber,
                InsuranceValidUpto = rc.InsuranceValidUpto,
                PuccNumber = rc.PuccNumber,
                PuccUpto = rc.PuccUpto,
                BodyType = rc.BodyType,
                CylinderCount = rc.CylinderCount,
                FinancerName = rc.FinancerName,
                IsFinanced = rc.IsFinanced,
                ChassisNumber = rc.ChassisNumber,
                EngineNumber = rc.EngineNumber,
                FuelType = rc.FuelType,
                CubicCapacity = rc.CubicCapacity,
                GrossVehicleWeight = rc.GrossVehicleWeight,
                IsCommercial = rc.IsCommercial,
                ManufacturerName = rc.ManufacturerName,
                ManufacturingMonthYear = rc.ManufacturingMonthYear,
                NormsType = rc.NormsType,
                Model = rc.Model,
                OwnerCount = rc.OwnerCount,
                PresentAddress = rc.PresentAddress,
                PermanentAddress = rc.PermanentAddress,
                RcExpiryDate = rc.RcExpiryDate,
                RegistrationAuthority = rc.RegistrationAuthority,
                RegistrationDate = rc.RegistrationDate,
                VehicleClass = rc.VehicleClass,
                VehicleCategory = rc.VehicleCategory,
                VehicleTaxUpto = rc.VehicleTaxUpto,
                VehicleColor = rc.VehicleColor,
                RtoCode = rc.RtoCode,
                SeatCapacity = rc.SeatCapacity,
                UnladenWeight = rc.UnladenWeight,
                Wheelbase = rc.Wheelbase,
                CreatedAt = DateTime.UtcNow,
                RawApiResponse = rc.RawApiResponse
            };
        }
    }
}
