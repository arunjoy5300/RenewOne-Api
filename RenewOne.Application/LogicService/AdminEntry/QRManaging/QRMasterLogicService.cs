using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.QRMapping;
using RenewOne.Application.DTOs.Response.QRMapping;
using RenewOne.Application.Filters.QRMaster;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.AdminEntry.QRManaging;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.QRManaging;
using RenewOne.Application.Interfaces.Repositories.BankAccounts;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.AdminEntry.QRManaging
{
    public class QRMasterLogicService : IQRMasterLogicService
    {
        #region DI
        private readonly IQRMasterRepository _qrMasterRepository;
        private readonly ILogger<QRMasterLogicService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBankAccountRepository _bankAccountRepository;
        #endregion

        #region Constructor
        public QRMasterLogicService(IQRMasterRepository qrMasterRepository, ILogger<QRMasterLogicService> logger, IUnitOfWork unitOfWork, IBankAccountRepository bankAccountRepository)
        {
            _qrMasterRepository = qrMasterRepository;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _bankAccountRepository = bankAccountRepository;
        }
        #endregion

        #region Create QR Master
        public async Task<Result<Guid>> CreateAsync(CreateQRMasterRequest request,CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating QR Master.");

                var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId);

                if (bankAccount == null)
                {
                    _logger.LogWarning("Bank account not found. Id: {Id}", request.BankAccountId);
                    return Result<Guid>.NotFound("Bank account not found.");
                }

                var duplicateQr = await _qrMasterRepository
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.BankAccountId == request.BankAccountId &&
                        x.UpiId == request.UpiId,
                        cancellationToken);

                if (duplicateQr)
                {
                    _logger.LogWarning(
                        "QR Master already exists. BankAccountId: {BankAccountId}, UpiId: {UpiId}",
                        request.BankAccountId, request.UpiId);

                    return Result<Guid>.Failure(
                        "A QR with the same UPI ID already exists for this bank account.");
                }

                await _unitOfWork.BeginTransactionAsync();

                var qrMaster = new QRMaster
                {
                    Id = Guid.NewGuid(),
                    BankAccountId = request.BankAccountId,
                    UpiId = request.UpiId,
                    UpiName = request.UpiName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _qrMasterRepository.AddAsync(qrMaster);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "QR Master created successfully. Id: {Id}",
                    qrMaster.Id);

                return Result<Guid>.Success(
                    qrMaster.Id,
                    "QR Master created successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(ex, "Error creating QR Master.");

                return Result<Guid>.Failure(
                    "An error occurred while creating QR Master.",
                    500);
            }
        }
        #endregion

        #region Get All QR Masters
        public async Task<Result<PagedResponse<QRMasterResponse>>> GetAllAsync(QRMasterFilter filter,CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching QR Masters.");

                var query = _qrMasterRepository.GetQueryable();

                if (!string.IsNullOrWhiteSpace(filter.Search))
                {
                    query = query.Where(x =>
                        x.UpiName.Contains(filter.Search) ||
                        x.BankAccount.BankName.Contains(filter.Search) ||
                        x.BankAccount.AccountName.Contains(filter.Search));
                }

                if (filter.BankAccountId.HasValue)
                {
                    query = query.Where(x =>
                        x.BankAccountId == filter.BankAccountId.Value);
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(x =>
                        x.IsActive == filter.IsActive.Value);
                }

                query = filter.SortDescending
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt);

                var response = query.Select(x => new QRMasterResponse
                {
                    Id = x.Id,
                    BankAccountId = x.BankAccountId,
                    BankAccount = x.BankAccount.AccountName,
                    UpiName = x.UpiName,
                    UpiId = x.UpiId,
                    IsActive = x.IsActive
                });

                var pagedResult = await response.ToPagedResponseAsync(
                    filter.PageNumber,
                    filter.PageSize);

                _logger.LogInformation(
                    "QR Masters fetched successfully.");

                return Result<PagedResponse<QRMasterResponse>>.Success(
                    pagedResult,
                    "QR Masters fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching QR Masters.");

                return Result<PagedResponse<QRMasterResponse>>.Failure(
                    "An error occurred while fetching QR Masters.",
                    500);
            }
        }
        #endregion

        #region Get QR Master By Id
        public async Task<Result<QRMasterResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Fetching QR Master. Id: {Id}",
                    id);

                var qrMaster = await _qrMasterRepository.GetByIdAsync(id);

                if (qrMaster == null)
                {
                    _logger.LogWarning(
                        "QR Master not found. Id: {Id}",
                        id);

                    return Result<QRMasterResponse>.NotFound(
                        "QR Master not found.");
                }

                var response = new QRMasterResponse
                {
                    Id = qrMaster.Id,
                    BankAccountId = qrMaster.BankAccountId,
                    BankAccount = qrMaster.BankAccount.AccountName,
                    UpiId = qrMaster.UpiId,
                    UpiName = qrMaster.UpiName,
                    IsActive = qrMaster.IsActive
                };

                _logger.LogInformation(
                    "QR Master fetched successfully. Id: {Id}",
                    id);

                return Result<QRMasterResponse>.Success(
                    response,
                    "QR Master fetched successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching QR Master. Id: {Id}",
                    id);

                return Result<QRMasterResponse>.Failure(
                    "An error occurred while fetching QR Master.",
                    500);
            }
        }
        #endregion

        #region Update QR Master
        public async Task<Result<string>> UpdateAsync(UpdateQRMasterRequest request,CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Updating QR Master. Id: {Id}",
                    request.Id);

                var qrMaster = await _qrMasterRepository.GetByIdAsync(request.Id);

                if (qrMaster == null)
                {
                    _logger.LogWarning(
                        "QR Master not found. Id: {Id}",
                        request.Id);

                    return Result<string>.NotFound("QR Master not found.");
                }

                var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId);

                if (bankAccount == null)
                {
                    _logger.LogWarning(
                        "Bank account not found. Id: {Id}",
                        request.BankAccountId);

                    return Result<string>.NotFound("Bank account not found.");
                }

                var duplicateQr = await _qrMasterRepository
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.Id != request.Id &&
                        x.BankAccountId == request.BankAccountId &&
                        x.UpiId == request.UpiId,
                        cancellationToken);

                if (duplicateQr)
                {
                    _logger.LogWarning(
                        "Duplicate QR Master found. BankAccountId: {BankAccountId}, UpiId: {UpiId}",
                        request.BankAccountId,
                        request.UpiId
                        );

                    return Result<string>.Failure(
                        "A QR with the same UpiId already exists for this bank account.");
                }

                await _unitOfWork.BeginTransactionAsync();

                qrMaster.BankAccountId = request.BankAccountId;
                qrMaster.UpiId = request.UpiId;
                qrMaster.UpiName = request.UpiName;
                qrMaster.IsActive = request.IsActive;
                qrMaster.UpdatedAt = DateTime.UtcNow;

                _qrMasterRepository.Update(qrMaster);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "QR Master updated successfully. Id: {Id}",
                    qrMaster.Id);

                return Result<string>.Success(
                    qrMaster.Id.ToString(),
                    "QR Master updated successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error updating QR Master. Id: {Id}",
                    request.Id);

                return Result<string>.Failure(
                    "An error occurred while updating QR Master.",
                    500);
            }
        }
        #endregion

        #region Toggle QR Master Status
        public async Task<Result<string>> ChangeStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Changing QR Master status. Id: {Id}, IsActive: {IsActive}",
                    id,
                    isActive);

                var qrMaster = await _qrMasterRepository.GetByIdAsync(id);

                if (qrMaster == null)
                {
                    _logger.LogWarning(
                        "QR Master not found. Id: {Id}",
                        id);

                    return Result<string>.NotFound("QR Master not found.");
                }

                if (qrMaster.IsActive == isActive)
                {
                    return Result<string>.Failure(
                        $"QR Master is already {(isActive ? "Active" : "Inactive")}.");
                }

                await _unitOfWork.BeginTransactionAsync();

                qrMaster.IsActive = isActive;
                qrMaster.UpdatedAt = DateTime.UtcNow;

                _qrMasterRepository.Update(qrMaster);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation(
                    "QR Master status updated successfully. Id: {Id}",
                    id);

                return Result<string>.Success(
                    id.ToString(),
                    $"QR Master {(isActive ? "activated" : "deactivated")} successfully.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                _logger.LogError(
                    ex,
                    "Error changing QR Master status. Id: {Id}",
                    id);

                return Result<string>.Failure(
                    "An error occurred while updating QR Master status.",
                    500);
            }
        }
        #endregion
    }
}
