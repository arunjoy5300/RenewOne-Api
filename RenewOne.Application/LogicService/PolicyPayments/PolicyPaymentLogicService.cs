using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.PolicyPayment;
using RenewOne.Application.DTOs.Response.QRManaging;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.PolicyPayments;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.EnquiryQuote;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.QRManaging;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.PolicyPayments
{
    public class PolicyPaymentLogicService : IPolicyPaymentLogicService
    {
        private readonly IPolicyPaymentRepository _policyPaymentRepository;
        private readonly ILogger<PolicyPaymentLogicService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IEnquiryRepository _enquiryRepository;
        private readonly IEnquiryQuoteRepository _enquiryQuoteRepository;
        private readonly IQRMappingRepository _qrMappingRepository;
        public PolicyPaymentLogicService(
            IPolicyPaymentRepository policyPaymentRepository,
            ILogger<PolicyPaymentLogicService> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IEnquiryRepository enquiryRepository,
            IEnquiryQuoteRepository enquiryQuoteRepository,
            IQRMappingRepository qrMappingRepository)
        {
            _policyPaymentRepository = policyPaymentRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _enquiryRepository = enquiryRepository;
            _enquiryQuoteRepository = enquiryQuoteRepository;
            _qrMappingRepository = qrMappingRepository;
        }

        public async Task<Result<string>> ApprovePolicyPaymentAsync(ApprovePolicyPaymentRequest request, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.LogInformation(
                    "Approving policy payment. PolicyPaymentId: {PolicyPaymentId}",
                    request.PolicyPaymentId);

                var policyPayment = await _policyPaymentRepository.GetByIdAsync(request.PolicyPaymentId);
                if(policyPayment == null)
                {
                    return Result<string>.NotFound($"No payment found for PolicyPaymentId: {request.PolicyPaymentId}");
                }
                if (policyPayment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    _logger.LogWarning(
                        "Policy payment not found. PolicyPaymentId: {PolicyPaymentId}",
                        request.PolicyPaymentId);

                    return Result<string>.NotFound("Policy payment not found.");
                }

                // Validate Status
                if (policyPayment.Status != "Pending")
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    _logger.LogWarning(
                        "Policy payment is already processed. PolicyPaymentId: {PolicyPaymentId}",
                        request.PolicyPaymentId);

                    return Result<string>.Failure(
                        "Only pending payments can be approved.");
                }

                // Update Policy Payment
                policyPayment.Status = "Approved";

                policyPayment.VerifiedbyUserId = _currentUserService.UserId;

                policyPayment.VerifiedAt = DateTime.UtcNow;

                policyPayment.Remarks = request.Remarks ?? "Approved";

                policyPayment.UpdatedAt = DateTime.UtcNow;

                _policyPaymentRepository.Update(policyPayment);

                // Update Enquiry
                policyPayment.Enquiry.Status = EnquiryStatus.PolicyInProgress.ToString();

                policyPayment.Enquiry.UpdatedAt = DateTime.UtcNow;

                _enquiryRepository.Update(policyPayment.Enquiry);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Payment approved successfully. EnquiryId: {EnquiryId}, PolicyPaymentId: {PolicyPaymentId}",
                    policyPayment.EnquiryId,
                    policyPayment.Id);

                return Result<string>.Success(
                    policyPayment.Id.ToString(),
                    "Payment approved successfully.");

            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error while approving policy payment.");

                return Result<string>.Failure(
                    "Error while approving policy payment.",
                    500);
            }
        }

        public async Task<Result<string>> RejectPolicyPaymentAsync(RejectPolicyPaymentRequest request, CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.LogInformation(
                    "Rejecting policy payment. PolicyPaymentId: {PolicyPaymentId}",
                    request.PolicyPaymentId);

                var policyPayment = await _policyPaymentRepository.GetByIdAsync(request.PolicyPaymentId);
                if (policyPayment == null)
                {
                    return Result<string>.NotFound($"No payment found for PolicyPaymentId: {request.PolicyPaymentId}");
                }
                if (policyPayment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    _logger.LogWarning(
                        "Policy payment not found. PolicyPaymentId: {PolicyPaymentId}",
                        request.PolicyPaymentId);

                    return Result<string>.NotFound("Policy payment not found.");
                }

                // Validate Status
                if (policyPayment.Status != "Pending")
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    _logger.LogWarning(
                        "Policy payment is already processed. PolicyPaymentId: {PolicyPaymentId}",
                        request.PolicyPaymentId);

                    return Result<string>.Failure(
                        "Only pending payments can be rejected.");
                }

                // Update Policy Payment
                policyPayment.Status = "Rejected";

                policyPayment.VerifiedbyUserId = _currentUserService.UserId;

                policyPayment.VerifiedAt = DateTime.UtcNow;

                policyPayment.Remarks = request.Remarks ?? "Rejected";
                policyPayment.UpdatedAt = DateTime.UtcNow;

                _policyPaymentRepository.Update(policyPayment);

                // Update Enquiry
                policyPayment.Enquiry.Status = EnquiryStatus.PolicyInProgress.ToString();

                policyPayment.Enquiry.UpdatedAt = DateTime.UtcNow;

                _enquiryRepository.Update(policyPayment.Enquiry);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "Payment rejected successfully. EnquiryId: {EnquiryId}, PolicyPaymentId: {PolicyPaymentId}",
                    policyPayment.EnquiryId,
                    policyPayment.Id);

                return Result<string>.Success(
                    policyPayment.Id.ToString(),
                    "Payment rejected successfully.");

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error while rejecting policy payment.");

                return Result<string>.Failure(
                    "Error while rejecting policy payment.",
                    500);
            }
        }

        public async Task<Result<QRSelectionResponse>> GetQRSelectionAsync(
        Guid enquiryId,
        CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching QR details for EnquiryId: {EnquiryId}",
                    enquiryId);

                var enquiry = await _enquiryRepository
                    .GetEnquiryQueryable()
                    .Include(x => x.VehicleEnquiryDetails)
                    .FirstOrDefaultAsync(x => x.Id == enquiryId, cancellationToken);

                if (enquiry == null)
                {
                    _logger.LogWarning(
                        "Enquiry not found. EnquiryId: {EnquiryId}",
                        enquiryId);

                    return Result<QRSelectionResponse>.NotFound(
                        "Enquiry not found.");
                }

                Guid insuranceTypeId = enquiry.InsuranceTypeId;

                Guid? vehicleTypeId = enquiry.VehicleEnquiryDetails?.VehicleTypeId;

                var mapping = await _qrMappingRepository
                    .GetQueryable()
                    .Include(x => x.QRMaster)
                        .ThenInclude(x => x.BankAccount)
                    .FirstOrDefaultAsync(x =>
                            x.IsActive &&
                            x.InsuranceTypeId == insuranceTypeId &&
                            x.VehicleTypeId == vehicleTypeId &&
                            x.QRMaster.IsActive &&
                            x.QRMaster.BankAccount.IsActive,
                        cancellationToken);

                if (mapping == null)
                {
                    _logger.LogWarning(
                        "QR Mapping not found for EnquiryId: {EnquiryId}",
                        enquiryId);

                    return Result<QRSelectionResponse>.NotFound(
                        "QR Mapping not found.");
                }

                var bank = mapping.QRMaster.BankAccount;

                var response = new QRSelectionResponse
                {
                    QRMasterId = mapping.QRMasterId,
                    QRName = mapping.QRMaster.UpiName,

                    BankAccountId = bank.Id,
                    BankName = bank.BankName,
                    BranchName = bank.BranchName,
                    AccountHolderName = bank.AccountHolderName,
                    AccountNumber = bank.AccountNumber,
                    IFSCCode = bank.IFSCCode,
                    UpiId = bank.UpiId
                };

                _logger.LogInformation(
                    "QR details fetched successfully for EnquiryId: {EnquiryId}",
                    enquiryId);

                return Result<QRSelectionResponse>.Success(
                    response,
                    "QR details fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching QR details for EnquiryId: {EnquiryId}",
                    enquiryId);

                return Result<QRSelectionResponse>.Failure(
                    "An error occurred while fetching QR details.",
                    500);
            }
        }
    }
}
