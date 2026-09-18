using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Vendor;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.Vendor
{
    public class VendorTransactionRepository : IVendorTransactionRepository
    {
        private readonly AppDbContext _dbContext;
        public VendorTransactionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<VendorTransaction> GetQueryable()
        {
            return _dbContext.VendorTransaction
                .AsNoTracking();
        }

        public async Task<VendorTransaction?> GetByIdAsync(
            Guid id,
            CancellationToken ct)
        {
            return await _dbContext.VendorTransaction
                .FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task AddAsync(
            VendorTransaction entity,
            CancellationToken ct)
        {
            await _dbContext.VendorTransaction.AddAsync(entity, ct);
        }

        public void Update(VendorTransaction entity)
        {
            _dbContext.VendorTransaction.Update(entity);
        }
    }
}
