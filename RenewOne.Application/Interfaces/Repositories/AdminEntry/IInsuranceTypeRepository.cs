using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.AdminEntry
{
    public interface IInsuranceTypeRepository
    {
        IQueryable<InsuranceType> GetQueryable();

        Task<InsuranceType?> GetByIdAsync(Guid id);
    }
}
