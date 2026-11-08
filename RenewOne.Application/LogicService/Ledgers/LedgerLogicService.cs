using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Wallet;
using RenewOne.Domain.Enums;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Ledger;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Application.Interfaces.Services.LoggedUser;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.Ledgers
{
    public class LedgerLogicService : ILedgerLogicService
    {
        private readonly ILedgerRepository _ledgerRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILedgerTransactionRepository _ledgerTransactionRepository;
        private readonly ICurrentUserService _currentUser;
        public LedgerLogicService(
            ILedgerRepository ledgerRepository,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            ILedgerTransactionRepository ledgerTransactionRepository,
            ICurrentUserService currentUser)
        {
            _ledgerRepository = ledgerRepository;
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _ledgerTransactionRepository = ledgerTransactionRepository;
            _currentUser = currentUser;
        }
        public async Task<Result<bool>> AdjustManualWalletAsync(WalletAdjustmentRequest request)
        {
            if (request.Amount <= 0)
                return Result<bool>.Failure("Enter a positive adjustment amount.", 400);
            if (string.IsNullOrWhiteSpace(request.Remarks))
                return Result<bool>.Failure("A reason is required for manual wallet adjustments.", 400);
            if (!Enum.IsDefined(request.TransactionType))
                return Result<bool>.Failure("Adjustment type must be Credit or Debit.", 400);
            var transactionStarted = false;
            try
            {
                var accountId = request.AccountId;
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    return Result<bool>.NotFound("Account not found");
                }

                var ledger = await _ledgerRepository.GetByAccountIdAsync(accountId);
                if (ledger == null)
                {
                    return Result<bool>.NotFound("Manual wallet not found");
                }

                var balanceBefore = ledger.AvailableBalance;
                if (request.TransactionType == WalletTransactionType.Debit && balanceBefore < request.Amount)
                    return Result<bool>.Failure("Insufficient manual wallet balance.", 400);

                await _unitOfWork.BeginTransactionAsync();
                transactionStarted = true;
                ledger.AvailableBalance += request.TransactionType == WalletTransactionType.Credit
                    ? request.Amount
                    : -request.Amount;
                ledger.UpdatedAt = DateTime.UtcNow;

                var ledgerTransaction = new LedgerTransaction
                {
                    Id = Guid.NewGuid(),
                    LedgerId = ledger.Id,
                    TransactionType = request.TransactionType.ToString(),
                    Amount = request.Amount,
                    BalanceBefore = balanceBefore,
                    BalanceAfter = ledger.AvailableBalance,
                    Remarks = request.Remarks,
                    CreatedAt = DateTime.UtcNow,
                    CreatedByIpAddress = _currentUser.IpAddress,
                    CreatedByUserId = _currentUser.UserId
                };

                await _ledgerRepository.Update(ledger);
                await _ledgerTransactionRepository.AddLedgerTransaction(ledgerTransaction);
                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true, request.TransactionType == WalletTransactionType.Credit
                    ? "Manual wallet credited successfully."
                    : "Manual wallet debited successfully.");

            }
            catch
            {
                if (transactionStarted)
                    await _unitOfWork.RollbackTransactionAsync();
                return Result<bool>.Failure("Unable to adjust the manual wallet.", 500);
            }
        }
    }
}
