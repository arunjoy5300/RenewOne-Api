using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Wallet
{
    public interface IWalletTransactionRepository
    {
        Task AddAsync(WalletTransaction transaction);

        IQueryable<WalletTransaction> GetQueryable();
    }
}
