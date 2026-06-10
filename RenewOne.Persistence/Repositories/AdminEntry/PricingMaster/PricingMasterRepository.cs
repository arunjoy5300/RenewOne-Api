using Microsoft.EntityFrameworkCore;
using RenewOne.Application.DTOs.Response.PricingMaster;
using RenewOne.Application.Interfaces.Repositories.AdminEntry.PricingMaster;

namespace RenewOne.Persistence.Repositories.AdminEntry.PricingMaster
{
    public class PricingMasterRepository : IPricingMasterRepository
    {
        private readonly AppDbContext _context;

        public PricingMasterRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> AddPricingMaster(Domain.Entities.PricingMaster pricingMaster)
        {
            await _context.PricingMasters.AddAsync(pricingMaster);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Domain.Entities.PricingMaster>> GetAllPricingMaster()
        {
            return await _context.PricingMasters
                .Include(p => p.AgentClass)
                .Include(p => p.VehicleType)
                .Include(p => p.InsuranceCompany)
                .ToListAsync();
        }

        public async Task<Domain.Entities.PricingMaster> GetPricingMasterByIdAsync(Guid id)
        {
            return await _context.PricingMasters
                .Include(p => p.AgentClass)
                .Include(p => p.VehicleType)
                .Include(p => p.InsuranceCompany)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<bool> UpdatePricingMaster(Domain.Entities.PricingMaster pricingMaster)
        {
            _context.PricingMasters.Update(pricingMaster);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
