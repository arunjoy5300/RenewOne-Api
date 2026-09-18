using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.Vendor
{
    public interface IVendorTransactionRepository
    {
        IQueryable<VendorTransaction> GetQueryable();

        Task<VendorTransaction?> GetByIdAsync(
            Guid id,
            CancellationToken ct);

        Task AddAsync(
            VendorTransaction entity,
            CancellationToken ct);

        void Update(VendorTransaction entity);
    }
}
