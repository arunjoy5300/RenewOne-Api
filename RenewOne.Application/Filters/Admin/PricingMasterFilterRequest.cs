using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.Filters.Admin
{
    public class PricingMasterFilterRequest: PagedRequest
    {
        public Guid? InsuranceCompanyId { get; set; }
        public Guid? AgentClassId { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public string? FuelType { get; set; }
        public bool? IsActive { get; set; }
    }
}
