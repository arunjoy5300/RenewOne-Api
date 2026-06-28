using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.UserLedger
{
    public interface ILedgerRepository
    {
        Task AddAsync(Ledger ledger);
        Task Update(Ledger ledger);
        Task<Ledger?> GetByAccountIdAsync(Guid accountId);
    }
}
