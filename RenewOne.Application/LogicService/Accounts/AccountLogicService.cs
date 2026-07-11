using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Account;
using RenewOne.Application.DTOs.Response.Account;
using RenewOne.Application.Filters.Account;
using RenewOne.Application.Interfaces.Common;
using RenewOne.Application.Interfaces.LogicService.Accounts;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.Class;
using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Application.Interfaces.Repositories.Wallet;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.Accounts
{
    public class AccountLogicService : IAccountLogicService
    {
        #region DI
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IAgentClassRepository _agentClassRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILedgerRepository _ledgerRepository;
        #endregion

        #region Constructor
        public AccountLogicService(
            IUserRepository userRepository, 
            IAccountRepository accountRepository, 
            IAgentClassRepository agentClassRepository,
            IWalletRepository walletRepository,
            IUnitOfWork unitOfWork,
            ILedgerRepository ledgerRepository)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
            _agentClassRepository = agentClassRepository;
            _walletRepository = walletRepository;
            _unitOfWork = unitOfWork;
            _ledgerRepository = ledgerRepository;
        }
        #endregion

        #region ApproveAgentAsync
        public async Task<Result<bool>> ApproveAgentAsync(Guid accountId)
        {
            try
            {
                await _accountRepository.ApproveAgentAsync(accountId);

                await _unitOfWork.SaveChangesAsync();
                return Result<bool>.Success(true, "Agent approved successfully");
            }
            catch (KeyNotFoundException ex)
            {
                return Result<bool>.NotFound(ex.Message);
            }
            catch (Exception ex)
            { 
                return Result<bool>.Failure("Error approving agent",500);
            }
        }
        #endregion

        #region GetPendingAgentsAsync
        public async Task<Result<IEnumerable<PendingAgentResponse>>> GetPendingAgentsAsync(PendingAgentFilterRequest filter)
        {
            var agents = await _accountRepository.GetPendingAgentsWithFilterAsync(filter.FilterType, filter.CustomDate);

            var response = agents.Select(a => new PendingAgentResponse
            {
                AccountId = a.Id,
                AgentName = a.AgentName,
                Email = a.Email,
                ContactNumber = a.ContactNumber,
                RequestedClass = a.AgentClass?.ClassName ?? "Unknown",
                CreatedAt = a.CreatedAt
            });

            return Result<IEnumerable<PendingAgentResponse>>.Success(response, "Fetched pending agents successfully");
        }
        #endregion

        #region RegisterAgentAsync
        public async Task<Result<bool>> RegisterAgentAsync(AgentSignupRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingUser = await _userRepository.GetByUserNameAsync(request.UserName);
                if (existingUser != null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Conflict("Username already exists");
                }

                var basicClass = _agentClassRepository.GetQueryable()
                    .Where(a => a.ClassName == "Basic")
                    .FirstOrDefault();

                if (basicClass == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<bool>.Failure("Basic agent class configuration missing", 500);
                }

                var newAccount = new Account
                {
                    Id = Guid.NewGuid(),
                    AgentName = request.AgentName,
                    Code = await GenerateUniqueAgentCodeAsync(),
                    AgentClassId = basicClass.Id,
                    Email = request.Email,
                    ContactNumber = request.ContactNumber,
                    Pincode = request.Pincode,
                    Address = request.Address
                };

                var newAgent = new User
                {
                    Id = Guid.NewGuid(),
                    AccountId = newAccount.Id,
                    UserName = request.UserName,
                    FullName = request.AgentName,
                    Email = request.Email,
                    PhoneNumber = request.ContactNumber,
                    UserType = "Agent",
                    IsActive = false,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
                };

                var wallet = new Domain.Entities.Wallet
                {
                    Id = Guid.NewGuid(),
                    AccountId = newAccount.Id,
                    Balance = 0,
                    IsActive = true
                };

                var ledger = new Ledger
                {
                    Id= Guid.NewGuid(),
                    AccountId = newAccount.Id,
                    CreditLimit = 0,
                    AvailableBalance = 0,
                    IsActive = true
                };

                await _accountRepository.AddAccountAsync(newAccount);
                await _userRepository.AddUserAsync(newAgent);
                await _walletRepository.AddAsync(wallet);
                await _ledgerRepository.AddAsync(ledger);

                await _unitOfWork.CommitTransactionAsync();

                return Result<bool>.Success(true, "Agent registered successfully", 201);
            }catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<bool>.Failure("An error occurred while registering the agent: " + ex.Message, 500);

            }
        }
        #endregion

        #region GenerateUniqueAgentCodeAsync
        private async Task<string> GenerateUniqueAgentCodeAsync()
        {
            var lastAccount = _accountRepository.GetQueryable()
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastAccount != null && !string.IsNullOrEmpty(lastAccount.Code))
            {
                var parts = lastAccount.Code.Split('-');
                if (parts.Length > 0 && int.TryParse(parts.Last(), out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            string year = DateTime.UtcNow.Year.ToString();
            return $"AGENT-{year}-{nextNumber:D4}";
        }
        #endregion
    }
}
