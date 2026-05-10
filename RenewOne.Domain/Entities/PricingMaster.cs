using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class PricingMaster : BaseEntity
    {
        public Guid InsuranceTypeId { get; set; }
        public Guid AgentClassId { get; set; }
        public Guid VehicleTypeId { get; set; }
        public int? MinCC { get; set; }
        public int? MaxCC { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal CompanyPayable { get; set; }
        public int? AgentPoints { get; set; }
        public decimal AdminProfit { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual InsuranceType? InsuranceType { get; set; }
        public virtual AgentClass? AgentClass { get; set; }
        public virtual VehicleType? VehicleType { get; set; }
    }
}
