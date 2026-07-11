using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.QRMapping;
using RenewOne.Application.DTOs.Response.QRMapping;
using RenewOne.Application.Filters.QRMaster;

namespace RenewOne.Application.Interfaces.LogicService.AdminEntry.QRManaging
{
    public interface IQRMasterLogicService
    {
        Task<Result<Guid>> CreateAsync(CreateQRMasterRequest request, CancellationToken cancellationToken);

        Task<Result<PagedResponse<QRMasterResponse>>> GetAllAsync(QRMasterFilter filter, CancellationToken cancellationToken);

        Task<Result<QRMasterResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        Task<Result<string>> UpdateAsync(UpdateQRMasterRequest request, CancellationToken cancellationToken);

        Task<Result<string>> ChangeStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    }
}
