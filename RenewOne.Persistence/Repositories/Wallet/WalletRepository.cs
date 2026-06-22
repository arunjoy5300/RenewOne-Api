using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Wallet;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Wallet
{
    public class WalletRepository : IWalletRepository
    {
        #region DI
        private readonly AppDbContext _dbContext;
        #endregion

        #region Constructor
        public WalletRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        #endregion

        public async Task AddAsync(Domain.Entities.Wallet wallet)
        {
            await _dbContext.Wallets.AddAsync(wallet);
        }

        public async Task<Domain.Entities.Wallet?> GetByAccountIdAsync(Guid accountId)
        {
            return await _dbContext.Wallets
                .FirstOrDefaultAsync(x => x.AccountId == accountId);
        }

        public async Task<Domain.Entities.Wallet?> GetByIdAsync(Guid walletId)
        {
            return await _dbContext.Wallets
                .FirstOrDefaultAsync(x => x.Id == walletId);
        }

        public async Task Update(Domain.Entities.Wallet wallet)
        {
            _dbContext.Wallets.Update(wallet);
        }
    }
}
