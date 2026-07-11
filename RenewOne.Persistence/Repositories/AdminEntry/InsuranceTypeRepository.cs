using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.AdminEntry
{
    public class InsuranceTypeRepository : IInsuranceTypeRepository
    {
        private readonly AppDbContext _context;

        public InsuranceTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<InsuranceType> GetQueryable()
        {
            return _context.InsuranceTypes.AsQueryable();
        }

        public async Task<InsuranceType?> GetByIdAsync(Guid id)
        {
            return await _context.InsuranceTypes
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
