using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Response.AdminEntry;

namespace RenewOne.Application.Interfaces.LogicService.AdminEntry
{
    public interface IInsuranceTypeLogicService
    {
        Task<Result<List<InsuranceTypeResponse>>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<Result<InsuranceTypeResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
    }
}
