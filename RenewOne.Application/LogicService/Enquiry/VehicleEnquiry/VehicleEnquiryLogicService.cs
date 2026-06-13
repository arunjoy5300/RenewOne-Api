using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Services.External.RC;
using RenewOne.Application.Interfaces.Services.RC;
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
        public async Task<Result<string>> CreateVehicleEnquiryAsync(CreateVehicleEnquiry request, Guid tenantId, Guid userId)
        {
            var formattedNumber = request.VehicleNumber?.ToUpper().Replace(" ", "");
            _logger.LogInformation("Enquiry received for vehicle number : {number}", formattedNumber);

            var existingEnquiry = _enquiryRepository.GetEnquiries().Any(x =>
                x.VehicleEnquiryDetails != null &&
                x.VehicleEnquiryDetails.VehicleDetails != null &&
                x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber == formattedNumber &&
                x.Status != "Completed" &&
                x.CreatedAt >= DateTime.UtcNow.AddDays(-180));

            if(existingEnquiry)
            {
                _logger.LogInformation("An enquiry for this vehicle already exists.");
                return Result<string>.Conflict("An enquiry for this vehicle already exists.");
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
            await _vehicleEnquiryRepository.SaveChangesAsync();

            _logger.LogInformation("Request Submission Mode {request}", request.SubmissionMode);

            if(request.SubmissionMode == SubmissionMode.Api)
            {
                _logger.LogInformation("Fetching vehicle details from database");
                var vehicle = await _vehicleEnquiryRepository.GetVehicleDetailsByNumberAsync(formattedNumber);

                if (vehicle == null)
                {
                    _logger.LogInformation("Fetching vehicle details from Vahan API");
                    var rcApiResponse = await _rcApiService.FetchVehicleDetailsAsync(formattedNumber);

                    if (rcApiResponse == null)
                    {
                        _logger.LogInformation("Unable to fetch vehicle details from RC API.Please try submitting manually");
                        return Result<string>.Failure("Unable to fetch vehicle details from RC API.Please try submitting manually", 400);
                    }

                    vehicle = new VehicleDetails
                    {
                        Id = Guid.NewGuid(),
                        VehicleNumber = formattedNumber,
                        OwnerName = rcApiResponse.OwnerName,
                        OwnerFatherName = rcApiResponse.OwnerFatherName,
                        MobileNumber = rcApiResponse.MobileNumber,
                        InsuranceCompanyName = rcApiResponse.InsuranceCompanyName,
                        InsurancePolicyNumber = rcApiResponse.InsurancePolicyNumber,
                        InsuranceValidUpto = rcApiResponse.InsuranceValidUpto,
                        PuccNumber = rcApiResponse.PuccNumber,
                        PuccUpto = rcApiResponse.PuccUpto,
                        BodyType = rcApiResponse.BodyType,
                        CylinderCount = rcApiResponse.CylinderCount,
                        FinancerName = rcApiResponse.FinancerName,
                        IsFinanced = rcApiResponse.IsFinanced,
                        ChassisNumber = rcApiResponse.ChassisNumber,
                        EngineNumber = rcApiResponse.EngineNumber,
                        FuelType = rcApiResponse.FuelType,
                        CubicCapacity = rcApiResponse.CubicCapacity,
                        GrossVehicleWeight = rcApiResponse.GrossVehicleWeight,
                        IsCommercial = rcApiResponse.IsCommercial,
                        ManufacturerName = rcApiResponse.ManufacturerName,
                        ManufacturingMonthYear = rcApiResponse.ManufacturingMonthYear,
                        NormsType = rcApiResponse.NormsType,
                        Model = rcApiResponse.Model,
                        OwnerCount = rcApiResponse.OwnerCount,
                        PresentAddress = rcApiResponse.PresentAddress,
                        PermanentAddress = rcApiResponse.PermanentAddress,
                        RcExpiryDate = rcApiResponse.RcExpiryDate,
                        RegistrationAuthority = rcApiResponse.RegistrationAuthority,
                        RegistrationDate = rcApiResponse.RegistrationDate,
                        VehicleClass = rcApiResponse.VehicleClass,
                        VehicleCategory = rcApiResponse.VehicleCategory,
                        VehicleTaxUpto = rcApiResponse.VehicleTaxUpto,
                        VehicleColor = rcApiResponse.VehicleColor,
                        RtoCode = rcApiResponse.RtoCode,
                        SeatCapacity = rcApiResponse.SeatCapacity,
                        UnladenWeight = rcApiResponse.UnladenWeight,
                        Wheelbase = rcApiResponse.Wheelbase,
                        CreatedAt = DateTime.UtcNow,
                        RawApiResponse = rcApiResponse.RawApiResponse
                    };

                    await _vehicleEnquiryRepository.AddVehicleDetailsAsync(vehicle);

                    var apienquiryDetails = new VehicleEnquiryDetails
                    {
                        Id = Guid.NewGuid(),
                        EnquiryId = enquiry.Id,
                        VehicleDetailsId = vehicle.Id,
                        VehicleTypeId = request.VehicleTypeId,
                        IdvValue = null,
                        IsRcOwnerChanged = request.IsRCOwnerChanged,
                        HasPreviousClaim = request.HasPreviousClaim,
                        NcbPercentage = request.NcbPercentage,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _vehicleEnquiryRepository.AddVehicleEnquiryDetailsAsync(apienquiryDetails);

                    _logger.LogInformation("Saving");
                    var success = await _vehicleEnquiryRepository.SaveChangesAsync();

                    if (!success)
                    {
                        _logger.LogInformation("Failed to create vehicle enquiry.");
                        return Result<string>.Failure("Failed to create vehicle enquiry.", 500);
                    }
                    _logger.LogInformation("Vehicle enquiry successfully registered.");
                    return Result<string>.Success("Vehicle enquiry successfully registered.");
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

                var manualSuccess = await _vehicleEnquiryRepository.SaveChangesAsync();

                if (!manualSuccess)
                    return Result<string>.Failure("Failed to create vehicle enquiry.", 500);

                return Result<string>.Success("Vehicle enquiry successfully registered.");
            }

            return Result<string>.Success("Vehicle enquiry successfully registered.");
        }
    }
}
