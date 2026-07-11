using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.AdminEntry
{
    public class VehicleTypeRepository : IVehicleTypeRepository
    {
        private readonly AppDbContext _context;

        public VehicleTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<VehicleType> GetQueryable()
        {
            return _context.VehicleTypes.AsQueryable();
        }

        public async Task<VehicleType?> GetByIdAsync(Guid id)
        {
            return await _context.VehicleTypes
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
