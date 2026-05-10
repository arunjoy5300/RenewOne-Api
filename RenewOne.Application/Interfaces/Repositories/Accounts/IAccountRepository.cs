using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Accounts
{
    public interface IAccountRepository
    {
        Task AddAccountAsync(Account account);
    }
}
