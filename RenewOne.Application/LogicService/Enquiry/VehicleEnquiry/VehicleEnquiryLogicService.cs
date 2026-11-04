using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Helper;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Document;
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
using RenewOne.Application.Interfaces.Repositories.BankAccounts;
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
        private readonly ILedgerTransactionRepository _ledgerTransactionRepository;
        private readonly IBankTransactionRepository _bankTransactionRepository;
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
            INotificationService notificationService,
            ILedgerTransactionRepository ledgerTransactionRepository,
            IBankTransactionRepository bankTransactionRepository)
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
            _ledgerTransactionRepository = ledgerTransactionRepository;
            _bankTransactionRepository = bankTransactionRepository;
        }
        #endregion

        #region CreateVehicleEnquiryAsync
        public async Task<Result<VehicleEnquiryResponse>> CreateVehicleEnquiryAsync(CreateVehicleEnquiry request, Guid tenantId, Guid userId)
        {
            var vehicleNumber = request.VehicleNumber?.ToUpper().Replace(" ", "");

            _logger.LogInformation("Enquiry received for vehicle : {VehicleNumber}", vehicleNumber);

            var existingEnquiry = _enquiryRepository.GetEnquiries()
                .FirstOrDefault(x =>
                    x.VehicleEnquiryDetails != null &&
                    x.VehicleEnquiryDetails.VehicleDetails != null &&
                    x.VehicleEnquiryDetails.VehicleDetails.VehicleNumber == vehicleNumber &&
                    ( x.Status != EnquiryStatus.PolicyIssued.ToString() && x.Status != EnquiryStatus.Cancelled.ToString() ) &&
                    x.CreatedAt >= DateTime.UtcNow.AddDays(-180));

            if (existingEnquiry != null)
            {
                return Result<VehicleEnquiryResponse>.Success(
                    new VehicleEnquiryResponse
                    {
                        EnquiryId = existingEnquiry.Id,
                        EnquiryNumber = existingEnquiry.EnquiryNumber,
                        Brand = existingEnquiry.VehicleEnquiryDetails.VehicleDetails.ManufacturerName,
                        CubicCapacity = existingEnquiry.VehicleEnquiryDetails.VehicleDetails.CubicCapacity,
                        FuelType = existingEnquiry.VehicleEnquiryDetails.VehicleDetails.FuelType,
                        ManufactureYear = existingEnquiry.VehicleEnquiryDetails.VehicleDetails.ManufacturingMonthYear,
                        Model = existingEnquiry.VehicleEnquiryDetails.VehicleDetails.Model,
                        CurrentStep = existingEnquiry.CurrentStep,
                        SelectedQuotedId = existingEnquiry.SelectedQuoteId,
                        EngineNumber = existingEnquiry.VehicleEnquiryDetails.VehicleDetails.EngineNumber,
                        ChassisNumber = existingEnquiry.VehicleEnquiryDetails.VehicleDetails.ChassisNumber
                    });
            }

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
                EnquiryNumber = GenerateEnquiryNumber.GenerateEnquiryNumberAsync(),
                TenantId = tenantId,
                CustomerName = request.CustomerName,
                CustomerNumber = request.CustomerNumber,
                InsuranceTypeId = request.InsuranceTypeId,
                CreatedByUserId = userId,
                Priority = request.Priority,
                Status = "Created",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CurrentStep = 2
            };

            if (_currentUser.Role == "SuperAdmin" || _currentUser.Role == "AdminEmployee" || _currentUser.Role == "Supervisor")
            {
                enquiry.AssignedEmployeeId = _currentUser.UserId;
                enquiry.AdminAssisted = true;
            }
            else
            {
                enquiry.AdminAssisted = false;
            }

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

            if (!success)
            {
                return Result<VehicleEnquiryResponse>.Failure(
                    "Failed to create vehicle enquiry.",
                    500);
            }

            var response = new VehicleEnquiryResponse
            {
                EnquiryId = enquiry.Id,
                EnquiryNumber = enquiry.EnquiryNumber,
                Brand = vehicle?.ManufacturerName,
                Model = vehicle?.Model,
                CubicCapacity = vehicle?.CubicCapacity,
                ManufactureYear = vehicle?.ManufacturingMonthYear,
                FuelType = vehicle?.FuelType,
                SubmissionType = request.MIPolicyType,
                EngineNumber = vehicle?.EngineNumber,
                ChassisNumber = vehicle?.ChassisNumber
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
                    IsActive = true,
                    UploadedUserId = _currentUser.UserId,
                    UploadUserIpAddress = _currentUser.IpAddress,
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
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Submitting enquiry. EnquiryId: {EnquiryId}", request.EnquiryId);

                var enquiry = await _enquiryRepository
                    .GetEnquiries()
                    .FirstOrDefaultAsync(x => x.Id == request.EnquiryId, cancellationToken);

                if (enquiry == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<EnquirySubmitResponse>.NotFound("Enquiry not found");
                }

                if (enquiry.Status != EnquiryStatus.Created.ToString())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<EnquirySubmitResponse>.Failure("Only created enquiries can be submitted.");
                }

                var policyType = enquiry.VehicleEnquiryDetails?.MIPolicyType;
                bool hasInstantQuotes = false;

                // 1. THIRD PARTY FLOW
                if (policyType == PolicyType.ThirdParty.ToString())
                {
                    var account = await _accountRepository.GetByIdAsync(accountId);
                    if (account == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<EnquirySubmitResponse>.NotFound("Account not found");
                    }

                    if (!decimal.TryParse(enquiry.VehicleEnquiryDetails?.VehicleDetails?.CubicCapacity, out var ccValue))
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<EnquirySubmitResponse>.Failure("Invalid Cubic Capacity");
                    }

                    var pricingMasters = (await _pricingMasterRepository.GetTPPricing(
                        enquiry.VehicleEnquiryDetails.VehicleTypeId,
                        account.AgentClassId,
                        (int)ccValue)).ToList();

                    if (pricingMasters.Count > 0)
                    {
                        foreach (var pricing in pricingMasters)
                        {
                            await _enquiryQuoteRepository.AddEnquiryQuoteAsync(new EnquiryQuote
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

                        hasInstantQuotes = true;
                        enquiry.Status = EnquiryStatus.Quoted.ToString();
                        enquiry.CurrentStep = 3;
                    }
                    else
                    {
                        hasInstantQuotes = false;
                        enquiry.Status = EnquiryStatus.Submitted.ToString();
                        enquiry.CurrentStep = 3;
                    }
                }
                // 2. COMPREHENSIVE / OWN DAMAGE FLOW
                else if (policyType == PolicyType.OwnDamaged.ToString() || policyType == PolicyType.Comprehensive.ToString())
                {
                    var validation = await ValidateMandatoryDocumentsAsync(request.EnquiryId);
                    if (!validation.IsSuccess)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<EnquirySubmitResponse>.Failure(validation.Message, validation.StatusCode);
                    }

                    enquiry.Status = EnquiryStatus.Submitted.ToString();
                    enquiry.CurrentStep = 3;
                }
                else
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<EnquirySubmitResponse>.Failure("Invalid policy type.");
                }

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
                    "SuperAdmin", "AdminEmployee", "Supervisor");

                return Result<EnquirySubmitResponse>.Success(
                    new EnquirySubmitResponse
                    {
                        EnquiryId = enquiry.Id,
                        Status = enquiry.Status,
                        RedirectToQuotes = hasInstantQuotes
                    },
                    hasInstantQuotes ? "Quotes generated successfully." : "Enquiry submitted successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error submitting enquiry. EnquiryId: {EnquiryId}", request.EnquiryId);
                return Result<EnquirySubmitResponse>.Failure("Error submitting enquiry.", 500);
            }
        }

        #endregion

        #region SubmitPolicyRequest
        public async Task<Result<string>> SubmitPolicyRequestAsync(
    SubmitPolicyRequest request,
    CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "User {UserId} with role {Role} from IP {IpAddress} initiated policy submission.",
                _currentUser.UserId,
                _currentUser.Role,
                _currentUser.IpAddress);

            try
            {
                Guid accountId;

                // 1. അക്കൗണ്ട് റോൾ പ്രകാരം നിശ്ചയിക്കുന്നു
                if (_currentUser.Role == "SuperAdmin" ||
                    _currentUser.Role == "AdminEmployee" ||
                    _currentUser.Role == "Supervisor")
                {
                    if (!request.AccountId.HasValue)
                    {
                        return Result<string>.Failure("AccountId is required for admin submission.", 400);
                    }

                    accountId = request.AccountId.Value;
                }
                else
                {
                    accountId = _currentUser.AccountId;
                }

                _logger.LogInformation(
                    "Submitting policy request. EnquiryId: {EnquiryId}, QuoteId: {QuoteId}, AccountId: {AccountId}",
                    request.EnquiryId,
                    request.QuoteId,
                    accountId);

                // 2. തിരഞ്ഞെടുത്ത ക്വോട്ട് പരിശോധിക്കുന്നു
                var enquiryQuote = await _enquiryQuoteRepository
                    .GetEnquiryQuoteQuery()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.QuoteId && x.EnquiryId == request.EnquiryId,
                        cancellationToken);

                if (enquiryQuote == null)
                {
                    _logger.LogWarning(
                        "Selected quote not found. EnquiryId: {EnquiryId}, QuoteId: {QuoteId}",
                        request.EnquiryId,
                        request.QuoteId);
                    return Result<string>.NotFound("No selected Quote for enquiry found");
                }

                // 3. എൻക്വയറി പരിശോധിക്കുന്നു
                var enquiry = await _enquiryRepository
                    .GetEnquiryQueryable()
                    .FirstOrDefaultAsync(x => x.Id == enquiryQuote.EnquiryId, cancellationToken);

                if (enquiry == null)
                {
                    _logger.LogWarning("Enquiry not found. EnquiryId: {EnquiryId}", request.EnquiryId);
                    return Result<string>.NotFound("Enquiry not found.");
                }

                // 4. നിർബന്ധിത ഡോക്യുമെന്റുകൾ വാലിഡേറ്റ് ചെയ്യുന്നു
                var documentValidation = await ValidateMandatoryDocumentsAsync(enquiry.Id);
                if (!documentValidation.IsSuccess)
                {
                    _logger.LogWarning(
                        "Document validation failed. EnquiryId: {EnquiryId}. Reason: {Reason}",
                        enquiry.Id,
                        documentValidation.Message);

                    return Result<string>.Failure(documentValidation.Message, documentValidation.StatusCode);
                }

                // 5. ആക്ടീവ് പേയ്‌മെന്റ് ഡ്യൂപ്ലിക്കേഷൻ ചെക്ക്
                var hasActivePayment = await _policyPaymentRepository
                    .GetPolicyPaymentQueryable()
                    .AnyAsync(x =>
                        x.EnquiryId == request.EnquiryId &&
                        x.Status != "Rejected" &&
                        x.Status != "Cancelled",
                        cancellationToken);

                if (hasActivePayment)
                {
                    _logger.LogWarning(
                        "Duplicate policy payment submission. EnquiryId: {EnquiryId}, QuoteId: {QuoteId}",
                        request.EnquiryId,
                        request.QuoteId);

                    return Result<string>.Failure("A payment request has already been submitted for this quote.");
                }

                // 6. പേയ്‌മെന്റ് മോഡ് അനുസരിച്ചുള്ള മുൻകൂർ പരിശോധനകൾ
                Ledger? ledger = null;
                decimal netPayableAmount = enquiryQuote.Premium - enquiryQuote.AgentPoints;

                if (request.UseLedger)
                {
                    ledger = await _ledgerRepository.GetByAccountIdAsync(accountId);

                    if (ledger == null)
                    {
                        _logger.LogWarning("Ledger not found. AccountId: {AccountId}", accountId);
                        return Result<string>.NotFound("No ledger account found");
                    }

                    if (ledger.AvailableBalance < netPayableAmount)
                    {
                        _logger.LogWarning(
                            "Insufficient ledger balance. AccountId: {AccountId}, Balance: {Balance}, Required: {Required}",
                            accountId,
                            ledger.AvailableBalance,
                            netPayableAmount);

                        return Result<string>.Failure("Insufficient ledger balance");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(request.TransactionId))
                    {
                        return Result<string>.Failure("Transaction Id is required", 400);
                    }

                    if (!request.BankAccountId.HasValue)
                    {
                        return Result<string>.Failure("Bank account is required for bank transfer.", 400);
                    }
                }

                // --- ഡാറ്റാബേസ് ട്രാൻസാക്ഷൻ ആരംഭിക്കുന്നു ---
                await _unitOfWork.BeginTransactionAsync();

                // 7. ലെഡ്ജർ അപ്‌ഡേഷൻ (യഥാർത്ഥത്തിൽ കുറയ്ക്കുന്ന Net തുക തന്നെ Transaction-ലും നൽകുന്നു)
                if (request.UseLedger && ledger != null)
                {
                    var balanceBefore = ledger.AvailableBalance;

                    ledger.AvailableBalance -= netPayableAmount;
                    ledger.UpdatedAt = DateTime.UtcNow;

                    await _ledgerRepository.Update(ledger);

                    var ledgerTransaction = new LedgerTransaction
                    {
                        Id = Guid.NewGuid(),
                        LedgerId = ledger.Id,
                        TransactionType = "Debit",
                        Amount = netPayableAmount,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = ledger.AvailableBalance,
                        EnquiryId = enquiry.Id,
                        ReferenceNo = $"PP{Random.Shared.Next(10, 100):D2}",
                        Remarks = $"Policy payment for enquiry {enquiry.Id} (Net premium after points)",
                        CreatedByUserId = _currentUser.UserId,
                        CreatedByIpAddress = _currentUser.IpAddress,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _ledgerTransactionRepository.AddLedgerTransaction(ledgerTransaction);
                }

                // 8. മുൻപത്തെ ക്വോട്ടുകളുടെ സെലക്ഷൻ ഒഴിവാക്കുന്നു
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

                // നിലവിലെ ക്വോട്ട് സെലക്ട് ചെയ്യുന്നു
                enquiryQuote.IsSelected = true;
                enquiryQuote.SelectedAt = DateTime.UtcNow;
                enquiryQuote.SelectedByUserId = _currentUser.UserId;
                enquiryQuote.UpdatedAt = DateTime.UtcNow;

                bool isAdminRole = _currentUser.Role == "SuperAdmin" ||
                                   _currentUser.Role == "AdminEmployee" ||
                                   _currentUser.Role == "Supervisor";

                // 9. പേയ്‌മെന്റ് റെക്കോർഡ് ക്രിയേറ്റ് ചെയ്യുന്നു
                var policyPayment = new PolicyPayment
                {
                    Id = Guid.NewGuid(),
                    EnquiryId = enquiry.Id,
                    EnquiryQuoteId = enquiryQuote.Id,
                    AccountId = accountId,
                    Amount = enquiryQuote.Premium,
                    PaymentMode = request.UseLedger ? "Ledger" : "BankTransfer",
                    TransactionId = request.TransactionId,
                    ReferenceNumber = $"PP{Random.Shared.Next(10, 100):D2}",
                    Status = isAdminRole ? "Approved" : "Pending",
                    Remarks = request.Remarks,
                    BankAccountId = request.BankAccountId,
                    SubmittedByUserId = _currentUser.UserId,
                    SubmittedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _policyPaymentRepository.AddAsync(policyPayment);

                // 10. ബാങ്ക് ട്രാൻസ്ഫർ റെക്കോർഡ്
                if (!request.UseLedger && request.BankAccountId.HasValue)
                {
                    var bankTransaction = new BankTransaction
                    {
                        Id = Guid.NewGuid(),
                        BankAccountId = request.BankAccountId.Value,
                        TransactionType = "Credit",
                        Amount = enquiryQuote.Premium,
                        Category = "PolicyPayment",
                        Description = $"Policy payment received for enquiry {enquiry.Id}",
                        PolicyPaymentId = policyPayment.Id,
                        VendorId = null,
                        SalaryId = null,
                        UserId = _currentUser.UserId,
                        TransactionDate = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _bankTransactionRepository.AddAsync(bankTransaction, cancellationToken);
                }

                // 11. എൻക്വയറി സ്റ്റാറ്റസും സ്റ്റെപ്പും അപ്‌ഡേറ്റ് ചെയ്യുന്നു
                if (isAdminRole)
                {
                    enquiry.Status = EnquiryStatus.PolicyInProgress.ToString();
                }
                else
                {
                    enquiry.Status = EnquiryStatus.PaymentVerificationPending.ToString();
                    enquiry.AssignedEmployeeId = null;
                }

                enquiry.UpdatedAt = DateTime.UtcNow;
                enquiry.SelectedQuoteId = enquiryQuote.Id;

                // പേയ്‌മെന്റ് സബ്മിറ്റ് ചെയ്തു കഴിഞ്ഞാൽ പോളിസി പ്രിവ്യൂ & അപ്‌ലോഡ് ചെയ്യുന്ന Step 5-ലേക്ക് മാറുന്നു
                enquiry.CurrentStep = 5;

                _enquiryRepository.Update(enquiry);

                // ട്രാൻസാക്ഷൻ കൺഫേം ചെയ്യുന്നു
                await _unitOfWork.CommitTransactionAsync();

                // 12. അഡ്മിൻ നോട്ടിഫിക്കേഷൻ അയക്കുന്നു
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

                return Result<string>.Failure("Error submitting policy request.", 500);
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
                        x.DocType == DocumentType.RC.ToString() &&
                        x.DocSide == DocumentSide.Front.ToString())
                },
                //new
                //{
                //    Name = "RC Back",
                //    Exists = documents.Any(x =>
                //        x.DocType == DocumentType.RC.ToString() &&
                //        x.DocSide == DocumentSide.Back.ToString())
                //},
                new
                {
                    Name = "ID Front",
                    Exists = documents.Any(x =>
                        x.DocType == DocumentType.Identity.ToString() &&
                        x.DocSide == DocumentSide.Front.ToString())
                },
                //new
                //{
                //    Name = "ID Back",
                //    Exists = documents.Any(x =>
                //        x.DocType == DocumentType.Identity.ToString() &&
                //        x.DocSide == DocumentSide.Back.ToString())
                //},
                //new
                //{
                //    Name = "Previous Policy",
                //    Exists = documents.Any(x =>
                //        x.DocType == DocumentType.PreviousPolicy.ToString())
                //}
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

        #region List Uploaded Documents
        public async Task<Result<IEnumerable<DocumentListDto>>> ListUploadedDocuments(Guid enquiryId, CancellationToken ct)
        {
            var documents = await _documentRepository.GetAllDocumentsByEnquiryId(enquiryId);

            if (documents == null || !documents.Any())
            {
                return Result<IEnumerable<DocumentListDto>>.Failure("No documents found.", 404);
            }

            var documentDtos = documents.Select(doc => new DocumentListDto
            {
                Id = doc.Id,
                FileName = doc.DocType + "_" + doc.DocSide,
                DocType = doc.DocType,
                DocSide = doc.DocSide,
                UploadedUserIpAddress = doc.UploadUserIpAddress,
                UploadedUserName = doc.UploadedUser?.FullName,
                Url = doc.Url
            }).ToList();

            return Result<IEnumerable<DocumentListDto>>.Success(documentDtos, "Documents fetched successfully");
        }
        #endregion

        #region Delete Uploaded Document
        public async Task<Result<bool>> DeleteUploadedDocument(Guid documentId, CancellationToken ct)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var document = await _documentRepository.GetDocuments().Where(x => x.Id == documentId && x.IsActive).FirstOrDefaultAsync(ct);
                if (document == null)
                {
                    return Result<bool>.Failure("Document not found.", 404);
                }

                document.IsActive = false;
                await _documentRepository.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true, "Document deleted successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting document with Id: {DocumentId}", documentId);
                return Result<bool>.Failure("Failed to delete document.", 500);
            }
        }
        #endregion

        #region Replace File
        public async Task<Result<bool>> ReplaceFile(ReplaceFileDto replaceFileDto, CancellationToken ct)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var oldDocument = await _documentRepository.GetDocuments()
                    .Where(x => x.Id == replaceFileDto.FileId && x.IsActive)
                    .FirstOrDefaultAsync(ct);

                if (oldDocument == null)
                {
                    return Result<bool>.Failure("Original document not found.", 404);
                }

                oldDocument.IsActive = false;

                var extension = Path.GetExtension(replaceFileDto.File.FileName);

                var key = $"vehicledocs/{replaceFileDto.FileId}/{Guid.NewGuid()}{extension}";

                await using var stream =
                    replaceFileDto.File.OpenReadStream();

                var path =
                    await _cloudflareR2Service.UploadFileAsync(
                        stream,
                        key,
                        replaceFileDto.File.ContentType);

                var newDocument = new Document
                {
                    EnquiryId = oldDocument.EnquiryId,
                    DocType = oldDocument.DocType,
                    DocSide = oldDocument.DocSide,

                    Url = path,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _documentRepository.AddAsync(newDocument);
                await _documentRepository.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true, "Document replaced successfully");

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error replacing file URLs.");
                return Result<bool>.Failure("Failed to replace file URLs.", 500);
            }
        }
        #endregion

        #region Add Vehicle Quote
        public async Task<Result<bool>> AddVehicleQuote(AddVehicleQuoteRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var enquiry = await _enquiryRepository.GetEnquiryQueryable()
                    .FirstOrDefaultAsync(x => x.Id == request.EnquiryId, cancellationToken);
                if (enquiry == null)
                {
                    return Result<bool>.Failure("Enquiry not found.", 404);
                }
                var quote = new EnquiryQuote
                {
                    Id = Guid.NewGuid(),
                    EnquiryId = request.EnquiryId,
                    InsuranceCompanyId = request.InsuranceCompanyId,
                    VendorId = request.VendorId,
                    Premium = request.Premium,
                    CompanyPayable = request.CompanyPayable,
                    AgentPoints = request.AgentPoints,
                    AdminProfit = request.AdminProfit,
                    Remarks = request.Remarks,
                    Idv = request.Idv,
                    IsManualQuote = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _enquiryQuoteRepository.AddEnquiryQuoteAsync(quote);
                await _enquiryQuoteRepository.SaveChangesAsync();
                return Result<bool>.Success(true, "Vehicle quote added successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding vehicle quote for EnquiryId: {EnquiryId}", request.EnquiryId);
                return Result<bool>.Failure("Failed to add vehicle quote.", 500);
            }
        }
        #endregion

        #region List Vehicle Quotes
        public async Task<Result<PagedResponse<QuoteDto>>> ListVehicleQuotes(
        ListVehicleQuotesRequest request,
        CancellationToken cancellationToken = default)
        {
            try
            {
                var enquiry = await _enquiryRepository.GetEnquiryQueryable()
                    .FirstOrDefaultAsync(x => x.Id == request.EnquiryId, cancellationToken);

                if (enquiry == null)
                {
                    return Result<PagedResponse<QuoteDto>>.Failure("Enquiry not found.", 404);
                }

                var query = _enquiryQuoteRepository.GetEnquiryQuoteQuery()
                    .Where(x => x.EnquiryId == request.EnquiryId);

                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var search = request.Search.Trim().ToLower();

                    query = query.Where(x =>
                        x.InsuranceCompany.CompanyName.ToLower().Contains(search));
                }

                query = request.SortBy?.ToLower() switch
                {
                    "insurancecompanyname" => request.SortDescending
                        ? query.OrderByDescending(x => x.InsuranceCompany.CompanyName)
                        : query.OrderBy(x => x.InsuranceCompany.CompanyName),

                    "premium" => request.SortDescending
                        ? query.OrderByDescending(x => x.Premium)
                        : query.OrderBy(x => x.Premium),

                    "idv" => request.SortDescending
                        ? query.OrderByDescending(x => x.Idv)
                        : query.OrderBy(x => x.Idv),

                    "agentpoints" => request.SortDescending
                        ? query.OrderByDescending(x => x.AgentPoints)
                        : query.OrderBy(x => x.AgentPoints),

                    "quotedate" => request.SortDescending
                        ? query.OrderByDescending(x => x.CreatedAt)
                        : query.OrderBy(x => x.CreatedAt),

                    _ => query.OrderByDescending(x => x.CreatedAt)
                };

                var totalCount = await query.CountAsync(cancellationToken);

                var quotes = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(x => new QuoteDto
                    {
                        Id = x.Id,
                        VendorId = x.VendorId,
                        VendorName = x.Vendor != null ? x.Vendor.Name : null,
                        InsuranceCompanyName = x.InsuranceCompany.CompanyName,
                        IDV = x.Idv,
                        Premium = x.Premium,
                        AgentPoints = x.AgentPoints,
                        Remarks = x.Remarks,
                        QuoteDate = x.CreatedAt,
                        IsSelected = x.IsSelected
                    })
                    .ToListAsync(cancellationToken);

                var response = new PagedResponse<QuoteDto>
                {
                    Items = quotes,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalRecords = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
                };

                return Result<PagedResponse<QuoteDto>>.Success(
                    response,
                    "Vehicle quotes listed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error listing vehicle quotes for EnquiryId: {EnquiryId}",
                    request.EnquiryId);

                return Result<PagedResponse<QuoteDto>>.Failure(
                    "Failed to list vehicle quotes.",
                    500);
            }
        }
        #endregion

        #region Submit Vehicle Quote by Admin
        public async Task<Result<bool>> SubmitVehicleQuoteByAdmin(
            Guid enquiryId,
            Guid quoteId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var enquiry = await _enquiryRepository.GetEnquiryQueryable()
                    .FirstOrDefaultAsync(x => x.Id == enquiryId, cancellationToken);
                if (enquiry == null)
                {
                    return Result<bool>.Failure("Enquiry not found.", 404);
                }
                var quote = await _enquiryQuoteRepository.GetEnquiryQuoteQuery()
                    .FirstOrDefaultAsync(x => x.Id == quoteId && x.EnquiryId == enquiryId, cancellationToken);
                if (quote == null)
                {
                    return Result<bool>.Failure("Quote not found.", 404);
                }
                // Unselect previous quotes
                var quotes = await _enquiryQuoteRepository.GetEnquiryQuoteQuery()
                    .Where(x => x.EnquiryId == enquiry.Id)
                    .ToListAsync(cancellationToken);
                foreach (var q in quotes)
                {
                    q.IsSelected = false;
                    q.SelectedByUserId = null;
                    q.SelectedAt = null;
                }
                // Select current quote
                quote.IsSelected = true;
                quote.SelectedAt = DateTime.UtcNow;
                quote.SelectedByUserId = _currentUser.UserId;
                quote.UpdatedAt = DateTime.UtcNow;
                // Update enquiry status
                enquiry.Status = EnquiryStatus.Quoted.ToString();
                enquiry.UpdatedAt = DateTime.UtcNow;
                enquiry.SelectedQuoteId = quote.Id;
                enquiry.CurrentStep = 4;
                _enquiryRepository.Update(enquiry);
                await _enquiryQuoteRepository.SaveChangesAsync();
                return Result<bool>.Success(true, "Vehicle quote submitted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error submitting vehicle quote for EnquiryId: {EnquiryId}, QuoteId: {QuoteId}",
                    enquiryId,
                    quoteId);
                return Result<bool>.Failure(
                    "Failed to submit vehicle quote.",
                    500);
            }
        }
        #endregion
    }
}
