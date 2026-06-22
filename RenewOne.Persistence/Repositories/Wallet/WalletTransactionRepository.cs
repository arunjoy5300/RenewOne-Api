using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Wallet;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Wallet
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly AppDbContext _dbContext;
        public WalletTransactionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(WalletTransaction transaction)
        {
            await _dbContext.WalletTransactions.AddAsync(transaction);
        }

        public IQueryable<WalletTransaction> GetQueryable()
        {
            return _dbContext.WalletTransactions.AsNoTracking();
        }
    }
}
