using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class PolicyDetails : BaseEntity
    {
        public Guid EnquiryId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public DateOnly PolicyStartDate { get; set; }
        public DateOnly PolicyEndDate { get; set; }
        public string? Remarks { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUserIpAddress { get; set; } = string.Empty;
        public virtual Enquiry Enquiry { get; set; } = null!;
        public virtual User CreatedByUser { get; set; } = null!;
    }
}
