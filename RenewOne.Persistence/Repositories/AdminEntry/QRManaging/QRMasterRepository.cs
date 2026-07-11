using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.QRManaging;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.AdminEntry.QRManaging
{
    public class QRMasterRepository : IQRMasterRepository
    {
        private readonly AppDbContext _dbContext;
        public QRMasterRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<QRMaster> GetQueryable()
        {
            return _dbContext.QRMasters
                .Include(x => x.BankAccount)
                .AsQueryable();
        }

        public async Task<QRMaster?> GetByIdAsync(Guid id)
        {
            return await _dbContext.QRMasters
                .Include(x => x.BankAccount)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddAsync(QRMaster qrMaster)
        {
            await _dbContext.QRMasters.AddAsync(qrMaster);
        }

        public void Update(QRMaster qrMaster)
        {
            _dbContext.QRMasters.Update(qrMaster);
        }
    }
}
