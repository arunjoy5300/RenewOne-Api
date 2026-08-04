using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Wallet;
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
        public async Task<Result<bool>> AddLedgerAsync(WalletRechargeRequest request, Guid accountId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                if (request.Amount <= 0)
                {
                    return Result<bool>.Failure("Please enter valid amount", 400);
                }
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    return Result<bool>.NotFound("Account not found");
                }

                var ledger = await _ledgerRepository.GetByAccountIdAsync(accountId);
                if (ledger == null)
                {
                    return Result<bool>.NotFound("No ledger account found");
                }

                var balanceBefore = ledger.AvailableBalance;

                ledger.CreditLimit += request.Amount;
                ledger.AvailableBalance += request.Amount;
                ledger.UpdatedAt = DateTime.UtcNow;

                var ledgerTransaction = new LedgerTransaction
                {
                    Id = Guid.NewGuid(),
                    LedgerId = ledger.Id,
                    TransactionType = "Credit",
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

                return Result<bool>.Success(true, "Amount added successfully");

            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<bool>.Failure($"Error adding ledger balance: {ex}", 500);
            }
        }
    }
}
