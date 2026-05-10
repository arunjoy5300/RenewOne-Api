using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class VehicleType : BaseEntity
    {
        public string TypeName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<VehicleEnquiryDetails> EnquiryDetails { get; set; } = new HashSet<VehicleEnquiryDetails>();
        public virtual ICollection<PricingMaster> PricingMasters { get; set; } = new HashSet<PricingMaster>();
    }
}
