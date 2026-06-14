using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.PricingMaster;
using RenewOne.Application.Interfaces.Repositories.Documents;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Services.External.R2;
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
        private readonly IFileStorageService _cloudflareR2Service;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPricingMasterRepository _pricingMasterRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IEnquiryQuoteRepository _enquiryQuoteRepository;
        public VehicleEnquiryLogicService(
            IVehicleEnquiryRepository vehicleEnquiryRepository, 
            IRCIDSService rcApiService,
            IEnquiryRepository enquiryRepository,
            ILogger<IVehicleEnquiryLogicService> logger,
            IFileStorageService cloudflareR2Service,
            IDocumentRepository documentRepository,
            IPricingMasterRepository pricingMasterRepository,
            IAccountRepository accountRepository,
            IEnquiryQuoteRepository enquiryQuoteRepository)
        {
            _vehicleEnquiryRepository = vehicleEnquiryRepository;
            _rcApiService = rcApiService;
            _enquiryRepository = enquiryRepository;
            _logger = logger;
            _cloudflareR2Service = cloudflareR2Service;
            _documentRepository = documentRepository;
            _pricingMasterRepository = pricingMasterRepository;
            _accountRepository = accountRepository;
            _enquiryQuoteRepository = enquiryQuoteRepository;
        }

        #region CreateVehicleEnquiryAsync
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
        #endregion

        #region MapVehicleDetails - Helper
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
        #endregion

        #region UploadDocumentAsync
        public async Task<Result<Guid>> UploadDocumentAsync(UploadVehicleDocumentRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var enquiry = _enquiryRepository.GetEnquiries().FirstOrDefault(x => x.Id == request.EnquiryId);

                if (enquiry == null)
                {
                    return Result<Guid>.Failure(
                        "Enquiry not found",
                         404);
                }

                var extension = Path.GetExtension(request.File.FileName);

                var key = $"vehicledocs/{request.EnquiryId}/{Guid.NewGuid()}{extension}";

                await using var stream =
                    request.File.OpenReadStream();

                var path =
                    await _cloudflareR2Service.UploadFileAsync(
                        stream,
                        key,
                        request.File.ContentType);

                var document = new Document
                {
                    Id = Guid.NewGuid(),
                    EnquiryId = request.EnquiryId,
                    DocType = request.DocType,
                    DocSide = request.DocSide,
                    Url = path,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _documentRepository.AddAsync(document);

                await _documentRepository.SaveChangesAsync();

                return Result<Guid>.Success(
                    document.Id,
                    "Document uploaded successfully");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file.");
                return Result<Guid>.Failure("Failed to upload file.", 500);
            }
        }
        #endregion

        public async Task<Result<EnquirySubmitResponse>> SubmitVehicleEnquiryAsync(SubmitVehicleEnquiry request, Guid AccountId,CancellationToken cancellationToken = default)
        {
            bool hasInstantQuotes = false;
            try
            {
                var enquiry = _enquiryRepository
                .GetEnquiries()
                .FirstOrDefault(x => x.Id == request.EnquiryId);

                if (enquiry == null)
                {
                    _logger.LogInformation("Enquiry not found");
                    return Result<EnquirySubmitResponse>.NotFound("Enquiry not found");
                }

                if(enquiry.Status != EnquiryStatus.Created.ToString())
                {
                    _logger.LogInformation("Only created enquiries can be submitted");
                    return Result<EnquirySubmitResponse>.Failure("Only created enquiries can be submitted");
                }

                _logger.LogInformation("Proceeding with logic for PolicyType: {type}", enquiry?.VehicleEnquiryDetails?.MIPolicyType);
                if(enquiry?.VehicleEnquiryDetails?.MIPolicyType == PolicyType.ThirdParty.ToString())
                {
                    var account = await _accountRepository.GetByIdAsync(AccountId);
                    if(account == null)
                    {
                        return Result<EnquirySubmitResponse>.NotFound("Account not found");
                    }
                    var agentClassId = account.AgentClassId;
                    _logger.LogInformation("AgentClass Fetched");
                    
                    if(!decimal.TryParse(enquiry.VehicleEnquiryDetails?.VehicleDetails?.CubicCapacity, out var ccValue))
                    {
                        return Result<EnquirySubmitResponse>.Failure("Invalid Cubic Capacity");
                    }

                    var cc = (int)ccValue;
                    _logger.LogInformation("CC : {cc}", cc);

                    var pricingMasters = await _pricingMasterRepository.GetTPPricing(enquiry.VehicleEnquiryDetails.VehicleTypeId, agentClassId,cc);
                    if(!pricingMasters.Any())
                    {
                        _logger.LogInformation("No qoutes available for this enquiry");
                    }

                    hasInstantQuotes = true;
                    foreach (var pricing in pricingMasters)
                    {
                        await _enquiryQuoteRepository.AddEnquiryQuoteAsync(
                            new EnquiryQuote
                            {
                                Id = Guid.NewGuid(),
                                EnquiryId = enquiry.Id,
                                InsuranceCompanyId = pricing.InsuranceCompanyId,
                                Premium = pricing.TotalPremium,
                                CompanyPayable = pricing.CompanyPayable,
                                AgentPoints = pricing.AgentPoints,
                                AdminProfit = pricing.AdminProfit,
                                CreatedAt = DateTime.UtcNow
                            });
                    }

                    enquiry.Status = EnquiryStatus.Quoted.ToString();
                    enquiry.UpdatedAt = DateTime.UtcNow;

                    await _enquiryQuoteRepository.SaveChangesAsync();
                    return Result<EnquirySubmitResponse>.Success(
                    new EnquirySubmitResponse
                    {
                        EnquiryId = enquiry.Id,
                        Status = enquiry.Status,
                        RedirectToQuotes = hasInstantQuotes
                    });

                }

                if(enquiry.VehicleEnquiryDetails?.MIPolicyType == PolicyType.OwnDamaged.ToString() ||
                    enquiry.VehicleEnquiryDetails?.MIPolicyType == PolicyType.Comprehensive.ToString())
                {
                    var documents = await _documentRepository.GetAllDocumentsByEnquiryId(request.EnquiryId);

                    if(documents == null)
                    {
                        _logger.LogInformation("No documents available");
                    }

                    _logger.LogInformation("Documents: {val}", documents.Select(x => x.DocType).ToList());

                    var hasRcFront = documents.Any(x =>
                        x.DocType == ((int)DocumentType.RC).ToString() &&
                        x.DocSide == ((int)DocumentSide.Front).ToString());

                    var hasRcBack = documents.Any(x =>
                        x.DocType == ((int)DocumentType.RC).ToString() &&
                        x.DocSide == ((int)DocumentSide.Back).ToString());

                    var hasIdFront = documents.Any(x =>
                        x.DocType == ((int)DocumentType.Identity).ToString() &&
                        x.DocSide == ((int)DocumentSide.Front).ToString());

                    var hasIdBack = documents.Any(x =>
                        x.DocType == ((int)DocumentType.Identity).ToString() &&
                        x.DocSide == ((int)DocumentSide.Back).ToString());

                    var hasPreviousPolicy = documents.Any(x =>
                        x.DocType == ((int)DocumentType.PreviousPolicy).ToString());

                    var missingDocuments = new List<string>();

                    if (hasRcFront == false)
                        missingDocuments.Add("RC Front");

                    if (!hasRcBack)
                        missingDocuments.Add("RC Back");

                    if (!hasIdFront)
                        missingDocuments.Add("ID Front");

                    if (!hasIdBack)
                        missingDocuments.Add("ID Back");

                    if (!hasPreviousPolicy)
                        missingDocuments.Add("Previous Policy");

                    if(missingDocuments.Any())
                    {
                        _logger.LogInformation($"Missing Documents: {string.Join(",", missingDocuments)}");
                        return Result<EnquirySubmitResponse>.Failure($"Missing Documents: {string.Join(",", missingDocuments)}", 400);
                    }
                }

                enquiry.Status = EnquiryStatus.Submitted.ToString();
                enquiry.UpdatedAt = DateTime.UtcNow;

                await _vehicleEnquiryRepository.SaveChangesAsync();
                _logger.LogInformation("Enquiry status updated successfully");

                return Result<EnquirySubmitResponse>.Success(new EnquirySubmitResponse { EnquiryId = enquiry.Id, Status = enquiry.Status, RedirectToQuotes = false }, "Enquiry submitted successfully");
            }
            catch(Exception ex)
            {
                return Result<EnquirySubmitResponse>.Failure("Error submitting enquiry", 500);
                //throw;
            }
        }
    }
}
