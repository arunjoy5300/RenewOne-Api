using RenewOne.Application.Interfaces.Repositories.AdminEntry;
using RenewOne.Domain.Entities;

namespace RenewOne.Persistence.Repositories.AdminEntry
{
    public class InsuranceCompanyRepository : IInsuranceCompanyRepository
    {
        private readonly AppDbContext _dbContext;
        public InsuranceCompanyRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IQueryable<InsuranceCompany> GetQueryable()
        {
            return _dbContext.InsuranceCompanies.AsQueryable();
        }
    }
}
