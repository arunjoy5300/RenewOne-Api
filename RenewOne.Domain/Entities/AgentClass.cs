using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class AgentClass : BaseEntity
    {
        public string ClassName { get; set; } = string.Empty;
        public decimal CommSharePercentage { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<Account> Accounts { get; set; } = new HashSet<Account>();
        public virtual ICollection<PricingMaster> PricingMasters { get; set; } = new HashSet<PricingMaster>();
    }
}
