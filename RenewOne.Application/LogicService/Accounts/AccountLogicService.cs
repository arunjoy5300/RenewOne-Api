using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Account;
using RenewOne.Application.DTOs.Response.Account;
using RenewOne.Application.Filters.Account;
using RenewOne.Application.Interfaces.LogicService.Accounts;
using RenewOne.Application.Interfaces.Repositories.Accounts;
using RenewOne.Application.Interfaces.Repositories.Users;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.LogicService.Accounts
{
    public class AccountLogicService : IAccountLogicService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAccountRepository _accountRepository;
        public AccountLogicService(IUserRepository userRepository, IAccountRepository accountRepository)
        {
            _userRepository = userRepository;
            _accountRepository = accountRepository;
        }

        public async Task<Result<bool>> ApproveAgentAsync(Guid accountId)
        {
            var result = await _accountRepository.ApproveAgentAsync(accountId);

            if (!result) 
                return Result<bool>.NotFound("Account not found");

            return Result<bool>.Success(true, "Agent approved successfully");
        }

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

        public async Task<Result<bool>> RegisterAgentAsync(AgentSignupRequest request)
        {
            var existingUser = await _userRepository.GetByUserNameAsync(request.UserName);
            if ( existingUser != null)
                return Result<bool>.Conflict("Username already exists");

            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                AgentName = request.AgentName,
                Code = request.Code,
                AgentClassId = request.AgentClassId,
                Email = request.Email,
                ContactNumber = request.ContactNumber,
                Pincode = request.Pincode,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var newAgent = new User
            {
                Id = Guid.NewGuid(),
                AccountId = newAccount.Id,
                UserName = request.UserName,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.ContactNumber,
                UserType = "Agent",
                IsActive = false,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _accountRepository.AddAccountAsync(newAccount);
            await _userRepository.AddUserAsync(newAgent);
            return Result<bool>.Success(true, "Agent registered successfully", 201);
        }
    }
}
