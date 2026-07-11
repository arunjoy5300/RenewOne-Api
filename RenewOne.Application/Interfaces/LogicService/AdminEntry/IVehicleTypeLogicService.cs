using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.AdminEntry;

namespace RenewOne.Application.Interfaces.LogicService.AdminEntry
{
    public interface IVehicleTypeLogicService
    {
        Task<Result<List<VehicleTypeResponse>>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<Result<VehicleTypeResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
