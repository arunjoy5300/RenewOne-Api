

using RenewOne.Application.DTOs.Response.PricingMaster;

namespace RenewOne.Application.Interfaces.Repositories.AdminEntry.PricingMaster
{
    public interface IPricingMasterRepository
    {
        Task<Domain.Entities.PricingMaster> GetPricingMasterByIdAsync(Guid id);
        Task<List<Domain.Entities.PricingMaster>> GetAllPricingMaster();
        Task<bool> AddPricingMaster(Domain.Entities.PricingMaster pricingMaster);
        Task<bool> UpdatePricingMaster(Domain.Entities.PricingMaster pricingMaster);
        Task<List<Domain.Entities.PricingMaster>> GetTPPricing(Guid vehicleTypeId, Guid agentClassId, int cc);
    }
}
