using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.QRMapping;
using RenewOne.Application.DTOs.Response.QRMapping;
using RenewOne.Application.Filters.QRMapping;

namespace RenewOne.Application.Interfaces.LogicService.AdminEntry.QRManaging
{
    public interface IQRMappingLogicService
    {
        Task<Result<Guid>> CreateAsync(
            CreateQRMappingRequest request,
            CancellationToken cancellationToken = default);

        Task<Result<PagedResponse<QRMappingResponse>>> GetAllAsync(
            QRMappingFilter filter,
            CancellationToken cancellationToken = default);

        Task<Result<QRMappingResponse>> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Result<string>> UpdateAsync(
            UpdateQRMappingRequest request,
            CancellationToken cancellationToken = default);

        Task<Result<string>> ChangeStatusAsync(
            Guid id,
            bool isActive,
            CancellationToken cancellationToken = default);
    }
}
