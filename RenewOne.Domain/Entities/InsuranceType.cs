using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class InsuranceType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<UserTaskAllocation> UserTaskAllocations { get; set; }
        = new HashSet<UserTaskAllocation>();
        public virtual ICollection<Enquiry> Enquiries { get; set; } = new HashSet<Enquiry>();
        public virtual ICollection<QRMapping> QRMappings { get; set; }
        = new HashSet<QRMapping>();
        public virtual ICollection<InsuranceProductDisplay> InsuranceProductDisplays { get; set; }
    }
}
