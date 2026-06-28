using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.UserLedger;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.UserLedger
{
    public class LedgerRepository : ILedgerRepository
    {
        private readonly AppDbContext _dbContext;
        public LedgerRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Ledger?> GetByAccountIdAsync(Guid accountId)
        {
            return await _dbContext.Ledgers
                .FirstOrDefaultAsync(x => x.AccountId == accountId);
        }
        public async Task AddAsync(Ledger ledger)
        {
            await _dbContext.Ledgers.AddAsync(ledger);
        }
        public async Task Update(Ledger ledger)
        {
            _dbContext.Ledgers.Update(ledger);
        }
    }
}
