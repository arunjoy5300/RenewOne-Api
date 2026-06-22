using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Wallet
{
    public interface IWalletRepository
    {
        Task AddAsync(Domain.Entities.Wallet wallet);
        Task<Domain.Entities.Wallet?> GetByAccountIdAsync(Guid accountId);
        Task<Domain.Entities.Wallet?> GetByIdAsync(Guid walletId);
        Task Update(Domain.Entities.Wallet wallet);
    }
}
