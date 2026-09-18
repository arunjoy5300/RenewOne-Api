using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.Vendor;

namespace RenewOne.Persistence.Repositories.Vendor
{
    public class VendorRepository : IVendorRepository
    {
        private readonly AppDbContext _dbContext;
        public VendorRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<Domain.Entities.Vendor> GetQueryable()
        {
            return _dbContext.Vendors
                .AsNoTracking();
        }
        public async Task AddAsync(Domain.Entities.Vendor entity,CancellationToken ct)
        {
            await _dbContext.Vendors.AddAsync(entity, ct);
        }
        public void Update(Domain.Entities.Vendor entity, CancellationToken ct = default)
        {
            _dbContext.Vendors.Update(entity);
        }
    }
}
