using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.AdminEntry
{
    public interface IInsuranceCompanyRepository
    {
        IQueryable<InsuranceCompany> GetQueryable();
    }
}
