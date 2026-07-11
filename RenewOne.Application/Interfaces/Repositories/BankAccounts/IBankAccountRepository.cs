using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.BankAccounts
{
    public interface IBankAccountRepository
    {
        IQueryable<BankAccount> GetQueryable();

        Task<BankAccount?> GetByIdAsync(Guid id);

        Task AddAsync(BankAccount bankAccount);

        void Update(BankAccount bankAccount);
    }
}
