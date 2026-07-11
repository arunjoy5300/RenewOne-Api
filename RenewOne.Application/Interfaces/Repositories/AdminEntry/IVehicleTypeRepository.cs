using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.Repositories.AdminEntry
{
    public interface IVehicleTypeRepository
    {
        IQueryable<VehicleType> GetQueryable();

        Task<VehicleType?> GetByIdAsync(Guid id);
    }
}
