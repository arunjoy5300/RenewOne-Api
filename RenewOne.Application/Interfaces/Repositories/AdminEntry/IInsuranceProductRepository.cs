using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.AdminEntry
{
    public interface IInsuranceProductRepository
    {
        IQueryable<InsuranceProductDisplay> GetInsuranceProducts();
    }
}
