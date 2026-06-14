using RenewOne.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace RenewOne.Domain.Entities
{
    [Table("PricingMasters")]
    public class PricingMaster : BaseEntity
    {
        public Guid InsuranceCompanyId { get; set; }
        public Guid AgentClassId { get; set; }
        public Guid VehicleTypeId { get; set; }
        public int? MinCC { get; set; }
        public int? MaxCC { get; set; }
        public decimal TotalPremium { get; set; }
        public decimal CompanyPayable { get; set; }
        public decimal AgentPoints { get; set; }
        public decimal AdminProfit { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual AgentClass? AgentClass { get; set; }
        public virtual VehicleType? VehicleType { get; set; }
        public virtual InsuranceCompany? InsuranceCompany { get; set; }
    }
}
