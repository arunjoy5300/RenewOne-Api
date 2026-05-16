using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Accounts
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(Guid id);
        Task AddAccountAsync(Account account);
        Task<bool> ApproveAgentAsync(Guid id);
        Task<IEnumerable<Account>> GetPendingAgentsWithFilterAsync(string filterType, DateTime? customDate);
    }
}
