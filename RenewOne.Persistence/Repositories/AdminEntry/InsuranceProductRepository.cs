using Microsoft.EntityFrameworkCore;
using RenewOne.Application.Interfaces.Repositories.AdminEntry;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.AdminEntry
{
    public class InsuranceProductRepository : IInsuranceProductRepository
    {
        private readonly AppDbContext _dbContext;
        public InsuranceProductRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<InsuranceProductDisplay> GetInsuranceProducts()
        {
            return _dbContext.InsuranceProductDisplays.AsNoTracking();
        }
    }
}
