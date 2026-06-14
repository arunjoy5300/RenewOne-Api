using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.PricingMaster;
using RenewOne.Application.DTOs.Response.PricingMaster;
using RenewOne.Domain.Entities;

namespace RenewOne.Application.Interfaces.LogicService.AdminEntry
{
    public interface IPricingMasterLogicService
    {
        Task<Result<bool>> CreatePricingAsync(PricingMasterRequest request);
        Task<Result<List<PricingMasterResponse>>> GetAllPricingAsync();
        Task<Result<PricingMasterResponse>> GetPricingByIdAsync(Guid id);
        Task<Result<bool>> UpdatePricingAsync(Guid id, PricingMasterRequest request);
    }
}
