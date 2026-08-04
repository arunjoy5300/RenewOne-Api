using RenewOne.Application.Common.Models;
using RenewOne.Application.Common.Pagination;
using RenewOne.Application.DTOs.Request.PricingMaster;
using RenewOne.Application.DTOs.Response.PricingMaster;
using RenewOne.Application.Filters.Admin;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.LogicService.AdminEntry
{
    public interface IPricingMasterLogicService
    {
        Task<Result<bool>> CreatePricingAsync(PricingMasterRequest request);
        Task<Result<PagedResponse<PricingMasterResponse>>> GetAllPricingAsync(PricingMasterFilterRequest request);
        Task<Result<PricingMasterResponse>> GetPricingByIdAsync(Guid id);
        Task<Result<bool>> UpdatePricingAsync(Guid id, PricingMasterRequest request);
    }
}
