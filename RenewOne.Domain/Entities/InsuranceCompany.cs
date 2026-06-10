using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class InsuranceCompany : BaseEntity
    {
        public string CompanyName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public virtual ICollection<PricingMaster>? PricingMasters { get; set; }
    }
}
