using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.UserLedger
{
    public interface ILedgerTransactionRepository
    {
        Task AddLedgerTransaction(LedgerTransaction ledgerTransaction);
    }
}
