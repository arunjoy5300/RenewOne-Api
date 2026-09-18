using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.BankAccounts
{
    public interface IBankTransactionRepository
    {
        IQueryable<BankTransaction> GetQueryable();

        Task<BankTransaction?> GetByIdAsync(
            Guid id,
            CancellationToken ct);

        Task AddAsync(
            BankTransaction transaction,
            CancellationToken ct);

        void Update(BankTransaction transaction);

        void Delete(BankTransaction transaction);
    }
}
