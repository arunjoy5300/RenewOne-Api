using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.Wallet;
using RenewOne.Application.DTOs.Response.Wallet;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Wallet;
using RenewOne.Application.Interfaces.Repositories.Wallet;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Domain.Entities;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.LogicService.Wallet
{
    public class WalletLogicService : IWalletlogicService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrentUserService _currentUser;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<WalletLogicService> _logger;
        public WalletLogicService(
            IWalletRepository walletRepository,
            ICurrentUserService currentUser,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork,
            ILogger<WalletLogicService> logger)
        {
            _walletRepository = walletRepository;
            _currentUser = currentUser;
            _walletTransactionRepository = walletTransactionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<bool>> AdjustWalletAsync(WalletAdjustmentRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.Amount <= 0)
                    return Result<bool>.Failure("Amount should be greater than zero.");

                _logger.LogInformation("Fetching wallet details");

                var wallet = await _walletRepository.GetByAccountIdAsync(request.AccountId);

                if (wallet == null)
                    return Result<bool>.NotFound("Wallet not found.");

                if (request.TransactionType == WalletTransactionType.Debit &&
                    wallet.Balance < request.Amount)
                {
                    _logger.LogError("Insufficient balance");
                    return Result<bool>.Failure("Insufficient wallet balance.");
                }

                _logger.LogInformation("Current Balance:{bal}", wallet.Balance);
                var balanceBefore = wallet.Balance;

                if (request.TransactionType == WalletTransactionType.Credit)
                    wallet.Balance += request.Amount;
                else
                    wallet.Balance -= request.Amount;

                wallet.UpdatedAt = DateTime.UtcNow;

                var transaction = new WalletTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,

                    TransactionType = request.TransactionType.ToString(),

                    Amount = request.Amount,

                    BalanceBefore = balanceBefore,
                    BalanceAfter = wallet.Balance,

                    Remarks = request.Remarks,

                    Status = "Success",

                    CreatedByUserId = _currentUser.UserId,

                    CreatedByIpAddress = _currentUser.IpAddress,

                    TransactionDate = DateTime.UtcNow,

                    ReferenceNo = $"WT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",

                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Updating Wallet");
                await _walletRepository.Update(wallet);

                _logger.LogInformation("Adding Wallet Transaction");
                await _walletTransactionRepository.AddAsync(transaction);

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(
                    true,
                    request.TransactionType == WalletTransactionType.Credit
                        ? "Wallet credited successfully."
                        : "Wallet debited successfully.");
            }
            catch(Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<bool>.Failure("Error adjusting wallet: " + ex.Message, 500);
            }
        }

        public async Task<Result<PagedResponse<WalletTransactionResponse>>> GetTransactionsAsync(WalletTransactionFilter filter)
        {
            var accountId = _currentUser.AccountId;

            var wallet = await _walletRepository.GetByAccountIdAsync(accountId);

            if (wallet == null)
                return Result<PagedResponse<WalletTransactionResponse>>
                    .NotFound("Wallet not found.");

            var query = _walletTransactionRepository
                .GetQueryable()
                .Where(x => x.WalletId == wallet.Id);

            // Transaction Type Filter
            if (filter.Type.HasValue)
            {
                query = query.Where(x => x.TransactionType == filter.Type.Value.ToString());
            }

            // Search
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim();
            }

            // Date Filter
            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(x => x.CreatedAt >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(x => x.CreatedAt <= to);
            }

            var totalRecords = await query.CountAsync();

            var transactions = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new WalletTransactionResponse
                {
                    Id = x.Id,
                    Type = Enum.Parse<WalletTransactionType>(x.TransactionType),
                    Amount = x.Amount,
                    BalanceAfter = x.BalanceAfter,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();

            var response = new PagedResponse<WalletTransactionResponse>
            {
                Items = transactions,
                TotalRecords = totalRecords,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return Result<PagedResponse<WalletTransactionResponse>>
                .Success(response);
        }

        public async Task<Result<WalletResponse>> GetWalletAsync()
        {
            try
            {
                var accountId = _currentUser.AccountId;

                var wallet = await _walletRepository.GetByAccountIdAsync(accountId);

                if (wallet == null)
                    return Result<WalletResponse>.NotFound("Wallet not found.");

                var response = new WalletResponse
                {
                    WalletId = wallet.Id,
                    Balance = wallet.Balance,
                    UpdatedAt = wallet.UpdatedAt
                };

                return Result<WalletResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<WalletResponse>.Failure("Error fetching wallet: " + ex.Message,500);
            }
        }
    }
}
