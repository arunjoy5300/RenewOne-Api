using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.Enquiry;
using RenewOne.Application.DTOs.Response.Enquiry.VehicleEnquiry;
using RenewOne.Application.DTOs.Response.RC;
using RenewOne.Application.Hubs.RenewOne.Application.Hubs;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.PricingMaster;
using RenewOne.Application.Interfaces.Repositories.Documents;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Application.Interfaces.Services.External.R2;
using RenewOne.Application.Interfaces.Services.External.RC;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Application.Interfaces.Services.SignalR;
using RenewOne.Domain.Entities;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.Enquiry.VehicleEnquiry
{
    public class VehicleEnquiryLogicService : IVehicleEnquiryLogicService
    {
        #region DI
        private readonly IVehicleEnquiryRepository _vehicleEnquiryRepository;
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly IRCIDSService _rcApiService;
        private readonly ILogger<IVehicleEnquiryLogicService> _logger;
        private readonly IFileStorageService _cloudflareR2Service;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPricingMasterRepository _pricingMasterRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IEnquiryQuoteRepository _enquiryQuoteRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILedgerRepository _ledgerRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IPolicyPaymentRepository _policyPaymentRepository;
        private readonly INotificationService _notificationService;
        #endregion

        #region Constructor
        public VehicleEnquiryLogicService(
            IVehicleEnquiryRepository vehicleEnquiryRepository,
            IRCIDSService rcApiService,
            IEnquiryRepository enquiryRepository,
            ILogger<IVehicleEnquiryLogicService> logger,
            IFileStorageService cloudflareR2Service,
            IDocumentRepository documentRepository,
            IPricingMasterRepository pricingMasterRepository,
            IAccountRepository accountRepository,
            IEnquiryQuoteRepository enquiryQuoteRepository,
            IUnitOfWork unitOfWork,
            ILedgerRepository ledgerRepository,
            ICurrentUserService currentUser,
            IPolicyPaymentRepository policyPaymentRepository,
            INotificationService notificationService)
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
            _unitOfWork = unitOfWork;
            _ledgerRepository = ledgerRepository;
            _currentUser = currentUser;
            _policyPaymentRepository = policyPaymentRepository;
            _notificationService = notificationService;
        }
        #endregion

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
        private static VehicleDetails MapVehicleDetails(RcApiDto rc, string vehicleNumber, string phoneNumber)
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

        #region SubmitVehicleEnquiryAsync
        public async Task<Result<EnquirySubmitResponse>> SubmitVehicleEnquiryAsync(
            SubmitVehicleEnquiry request,
            Guid accountId,
            CancellationToken cancellationToken = default)
        {
            bool hasInstantQuotes = false;

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.LogInformation(
                    "Submitting enquiry. EnquiryId: {EnquiryId}",
                    request.EnquiryId);

                var enquiry = _enquiryRepository
                    .GetEnquiries()
                    .FirstOrDefault(x => x.Id == request.EnquiryId);

                if (enquiry == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    _logger.LogWarning(
                        "Enquiry not found. EnquiryId: {EnquiryId}",
                        request.EnquiryId);

                    return Result<EnquirySubmitResponse>.NotFound("Enquiry not found");
                }

                if (enquiry.Status != EnquiryStatus.Created.ToString())
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    _logger.LogWarning(
                        "Invalid enquiry status. EnquiryId: {EnquiryId}, Status: {Status}",
                        enquiry.Id,
                        enquiry.Status);

                    return Result<EnquirySubmitResponse>.Failure(
                        "Only created enquiries can be submitted.");
                }

                if (enquiry.VehicleEnquiryDetails?.MIPolicyType ==
                    PolicyType.ThirdParty.ToString())
                {
                    var account = await _accountRepository.GetByIdAsync(accountId);

                    if (account == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<EnquirySubmitResponse>.NotFound("Account not found");
                    }

                    if (!decimal.TryParse(
                            enquiry.VehicleEnquiryDetails.VehicleDetails?.CubicCapacity,
                            out var ccValue))
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return Result<EnquirySubmitResponse>.Failure(
                            "Invalid Cubic Capacity");
                    }

                    var pricingMasters =
                        await _pricingMasterRepository.GetTPPricing(
                            enquiry.VehicleEnquiryDetails.VehicleTypeId,
                            account.AgentClassId,
                            (int)ccValue);

                    if (!pricingMasters.Any())
                    {
                        _logger.LogWarning(
                            "No pricing available. EnquiryId: {EnquiryId}",
                            enquiry.Id);
                    }

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
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                    }

                    hasInstantQuotes = pricingMasters.Any();

                    enquiry.Status = EnquiryStatus.Quoted.ToString();
                    enquiry.UpdatedAt = DateTime.UtcNow;

                    _enquiryRepository.Update(enquiry);

                    await _unitOfWork.CommitTransactionAsync();

                    await _notificationService.SendToRolesAsync(
                        new NotificationMessage
                        {
                            Event = "EnquirySubmitted",
                            Title = "Enquiry Submitted",
                            Message = $"A new enquiry has been submitted. EnquiryId: {enquiry.Id}"
                        },
                        "SuperAdmin",
                        "AdminEmployee",
                        "Supervisor");

                    _logger.LogInformation(
                        "Instant quotes generated successfully. EnquiryId: {EnquiryId}",
                        enquiry.Id);

                    return Result<EnquirySubmitResponse>.Success(
                        new EnquirySubmitResponse
                        {
                            EnquiryId = enquiry.Id,
                            Status = enquiry.Status,
                            RedirectToQuotes = hasInstantQuotes
                        },
                        "Quotes generated successfully.");
                }

                if (enquiry.VehicleEnquiryDetails?.MIPolicyType ==
                        PolicyType.OwnDamaged.ToString() ||
                    enquiry.VehicleEnquiryDetails?.MIPolicyType ==
                        PolicyType.Comprehensive.ToString())
                {
                    var validation =
                        await ValidateMandatoryDocumentsAsync(request.EnquiryId);

                    if (!validation.IsSuccess)
                    {
                        await _unitOfWork.RollbackTransactionAsync();

                        return Result<EnquirySubmitResponse>.Failure(
                            validation.Message,
                            validation.StatusCode);
                    }
                }

                enquiry.Status = EnquiryStatus.Submitted.ToString();
                enquiry.UpdatedAt = DateTime.UtcNow;

                _enquiryRepository.Update(enquiry);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Enquiry submitted successfully. EnquiryId: {EnquiryId}",
                    enquiry.Id);

                return Result<EnquirySubmitResponse>.Success(
                    new EnquirySubmitResponse
                    {
                        EnquiryId = enquiry.Id,
                        Status = enquiry.Status,
                        RedirectToQuotes = false
                    },
                    "Enquiry submitted successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error submitting enquiry. EnquiryId: {EnquiryId}",
                    request.EnquiryId);

                return Result<EnquirySubmitResponse>.Failure(
                    "Error submitting enquiry.",
                    500);
            }
        }

        #endregion

        #region SubmitPolicyRequest
        public async Task<Result<string>> SubmitPolicyRequestAsync(SubmitPolicyRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var accountId = _currentUser.AccountId;

                _logger.LogInformation(
                    "Submitting policy request. EnquiryId: {EnquiryId}, QuoteId: {QuoteId}, AccountId: {AccountId}",
                    request.EnquiryId,
                    request.QuoteId,
                    _currentUser.AccountId);

                var enquiryQuote = await _enquiryQuoteRepository
                    .GetEnquiryQuoteQuery()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.QuoteId &&
                            x.EnquiryId == request.EnquiryId,
                        cancellationToken);

                if (enquiryQuote == null)
                {
                    _logger.LogWarning(
                        "Selected quote not found. EnquiryId: {EnquiryId}, QuoteId: {QuoteId}",
                        request.EnquiryId,
                        request.QuoteId);
                    return Result<string>.NotFound("No selected Quote for enquiry found");
                }

                var enquiry = await _enquiryRepository
                    .GetEnquiryQueryable()
                    .FirstOrDefaultAsync(
                        x => x.Id == enquiryQuote.EnquiryId,
                        cancellationToken);

                if(enquiry == null)
                {
                    _logger.LogWarning(
                        "Enquiry not found. EnquiryId: {EnquiryId}",
                        request.EnquiryId);
                    return Result<string>.NotFound("Enquiry not found.");
                }

                var documentValidation =
                    await ValidateMandatoryDocumentsAsync(enquiry.Id);
                if (!documentValidation.IsSuccess)
                {
                    _logger.LogWarning(
                        "Document validation failed. EnquiryId: {EnquiryId}. Reason: {Reason}",
                        enquiry.Id,
                        documentValidation.Message);

                    return Result<string>.Failure(
                        documentValidation.Message,
                        documentValidation.StatusCode);
                }

                // Duplicate Check
                var hasActivePayment = await _policyPaymentRepository
                    .GetPolicyPaymentQueryable()
                    .AnyAsync(x =>
                        x.EnquiryId == request.EnquiryId &&
                        x.Status != "Rejected" &&
                        x.Status !=  "Cancelled");

                if (hasActivePayment)
                {
                    _logger.LogWarning(
                        "Duplicate policy payment submission. EnquiryId: {EnquiryId}, QuoteId: {QuoteId}",
                        request.EnquiryId,
                        request.QuoteId);

                    return Result<string>.Failure(
                        "A payment request has already been submitted for this quote.");
                }

                if (request.UseLedger)
                {
                    var ledger = await _ledgerRepository
                        .GetByAccountIdAsync(accountId);

                    if (ledger == null)
                    {
                        _logger.LogWarning(
                            "Ledger not found. AccountId: {AccountId}",
                            accountId);

                        return Result<string>.NotFound("No ledger account found");
                    }

                    if (ledger.AvailableBalance < enquiryQuote.Premium)
                    {
                        _logger.LogWarning(
                            "Insufficient ledger balance. AccountId: {AccountId}, Balance: {Balance}, Premium: {Premium}",
                            accountId,
                            ledger.AvailableBalance,
                            enquiryQuote.Premium);

                        return Result<string>.Failure("Insufficent ledger balance");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(request.TransactionId))
                    {
                        return Result<string>.Failure("Transaction Id is requied");
                    }
                }

                _logger.LogInformation(
                    "Creating policy payment. EnquiryId: {EnquiryId}",
                    enquiry.Id);

                // Start Transaction
                await _unitOfWork.BeginTransactionAsync();

                // Unselect previous quotes
                var quotes = await _enquiryQuoteRepository
                    .GetEnquiryQuoteQuery()
                    .Where(x => x.EnquiryId == enquiry.Id)
                    .ToListAsync(cancellationToken);

                foreach (var quote in quotes)
                {
                    quote.IsSelected = false;
                    quote.SelectedByUserId = null;
                    quote.SelectedAt = null;
                }

                // Select current quote
                enquiryQuote.IsSelected = true;
                enquiryQuote.SelectedAt = DateTime.UtcNow;
                enquiryQuote.SelectedByUserId = _currentUser.UserId;
                enquiryQuote.UpdatedAt = DateTime.UtcNow;

                // Create Policy Payment
                var policyPayment = new PolicyPayment
                {
                    Id = Guid.NewGuid(),
                    EnquiryId = enquiry.Id,
                    EnquiryQuoteId = enquiryQuote.Id,
                    AccountId = accountId,
                    Amount = enquiryQuote.Premium,
                    PaymentMode = request.UseLedger
                        ? "Ledger"
                        : "BankTransfer",
                    TransactionId = request.TransactionId,
                    Status = "Pending",
                    Remarks = request.Remarks,
                    SubmittedByUserId = _currentUser.UserId,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _policyPaymentRepository.AddAsync(policyPayment);

                _logger.LogInformation(
                    "Policy payment created successfully. PolicyPaymentId: {PolicyPaymentId}",
                    policyPayment.Id);

                enquiry.Status =
                    EnquiryStatus.PaymentVerificationPending.ToString();

                enquiry.UpdatedAt = DateTime.UtcNow;
                enquiry.SelectedQuoteId = enquiryQuote.Id;

                _enquiryRepository.Update(enquiry);

                await _unitOfWork.CommitTransactionAsync();

                await _notificationService.SendToRolesAsync(
                    new NotificationMessage
                    {
                        Event = "PolicyPaymentSubmitted",
                        Title = "Policy Payment Submitted",
                        Message = $"A new policy payment has been submitted. EnquiryId: {enquiry.Id}, PolicyPaymentId: {policyPayment.Id}"
                    },
                    "SuperAdmin",
                    "AdminEmployee",
                    "Supervisor");

                _logger.LogInformation(
                    "Policy submitted successfully. EnquiryId: {EnquiryId}, PolicyPaymentId: {PolicyPaymentId}",
                    enquiry.Id,
                    policyPayment.Id);

                return Result<string>.Success(
                    policyPayment.Id.ToString(),
                    "Policy submitted successfully.");

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error while submitting policy request. EnquiryId: {EnquiryId}, QuoteId: {QuoteId}",
                    request.EnquiryId,
                    request.QuoteId);

                return Result<string>.Failure(
                    "Error submitting policy request.",
                    500);
            }
        }
        #endregion

        #region MandatoryDocuments
        private async Task<Result<bool>> ValidateMandatoryDocumentsAsync(Guid enquiryId)
        {
            var documents = await _documentRepository.GetAllDocumentsByEnquiryId(enquiryId);

            if (documents == null || !documents.Any())
            {
                return Result<bool>.Failure("No documents uploaded.", 400);
            }

            var requiredDocuments = new[]
            {
                new
                {
                    Name = "RC Front",
                    Exists = documents.Any(x =>
                        x.DocType == ((int)DocumentType.RC).ToString() &&
                        x.DocSide == ((int)DocumentSide.Front).ToString())
                },
                new
                {
                    Name = "RC Back",
                    Exists = documents.Any(x =>
                        x.DocType == ((int)DocumentType.RC).ToString() &&
                        x.DocSide == ((int)DocumentSide.Back).ToString())
                },
                new
                {
                    Name = "ID Front",
                    Exists = documents.Any(x =>
                        x.DocType == ((int)DocumentType.Identity).ToString() &&
                        x.DocSide == ((int)DocumentSide.Front).ToString())
                },
                new
                {
                    Name = "ID Back",
                    Exists = documents.Any(x =>
                        x.DocType == ((int)DocumentType.Identity).ToString() &&
                        x.DocSide == ((int)DocumentSide.Back).ToString())
                },
                new
                {
                    Name = "Previous Policy",
                    Exists = documents.Any(x =>
                        x.DocType == ((int)DocumentType.PreviousPolicy).ToString())
                }
            };

            var missingDocuments = requiredDocuments
                .Where(x => !x.Exists)
                .Select(x => x.Name)
                .ToList();

            if (missingDocuments.Any())
            {
                var message = $"Missing Documents: {string.Join(", ", missingDocuments)}";

                _logger.LogInformation(message);

                return Result<bool>.Failure(message, 400);
            }

            return Result<bool>.Success(true);
        }
        #endregion

        #region PaymentDetails
        public async Task<Result<PaymentVerificationResponse>> GetPaymentDetails(Guid enquiryId)
        {
            var payment = await _policyPaymentRepository
                .GetQueryable()
                .FirstOrDefaultAsync(x => x.EnquiryId == enquiryId);

            if(payment == null)
            {
                _logger.LogInformation("No payment record found for EnquiryId: {enquiryId}",enquiryId);
                return Result<PaymentVerificationResponse>.NotFound("No payment record found");
            }

            var response = new PaymentVerificationResponse
            {
                CustomerName = payment.Enquiry.CustomerName,
                CustomerNumber = payment.Enquiry.CustomerNumber,
                VehicleNumber = payment.Enquiry.VehicleEnquiryDetails.VehicleDetails.VehicleNumber,
                PaymentMode = payment.PaymentMode,
                PaymentStatus = payment.Status,
                Premium = payment.Amount,
                Remarks = payment.Remarks,
                EnquiryId = payment.EnquiryId,
                PolicyPaymentId = payment.Id,
                SubmittedAt = payment.SubmittedAt,
                SubmittedBy = payment.SubmittedByUser.FullName,
                TransactionId = payment.TransactionId
            };

            return Result<PaymentVerificationResponse>.Success(response, "Payment details fetched successfully");
        }
        #endregion
    }
}
