using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.UserLedger
{
    public class LedgerTransactionRepository : ILedgerTransactionRepository
    {
        private readonly AppDbContext _dbContext;
        public LedgerTransactionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddLedgerTransaction(LedgerTransaction ledgerTransaction)
        {
            await _dbContext.LedgerTransactions.AddAsync(ledgerTransaction);
        }
    }
}
