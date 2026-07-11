using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.QRManaging;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.AdminEntry.QRManaging
{
    public class QRMappingRepository : IQRMappingRepository
    {
        private readonly AppDbContext _context;

        public QRMappingRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<QRMapping> GetQueryable()
        {
            return _context.QRMappings
                .Include(x => x.QRMaster)
                    .ThenInclude(x => x.BankAccount)
                .Include(x => x.InsuranceType)
                .Include(x => x.VehicleType)
                .AsQueryable();
        }

        public async Task<QRMapping?> GetByIdAsync(Guid id)
        {
            return await GetQueryable()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(QRMapping mapping)
        {
            await _context.QRMappings.AddAsync(mapping);
        }

        public void Update(QRMapping mapping)
        {
            _context.QRMappings.Update(mapping);
        }
    }
}
