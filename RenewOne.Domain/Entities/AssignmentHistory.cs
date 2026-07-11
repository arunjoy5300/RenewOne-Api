using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class AssignmentHistory : BaseEntity
    {
        public Guid EnquiryId { get; set; }
        public Guid ChangedByUserId { get; set; }
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; }
        public Guid? FromEmployeeId { get; set; }
        public Guid ToEmployeeId { get; set; }
        public string? Remarks { get; set; }
        public virtual Enquiry? Enquiry { get; set; }
        public virtual User? ChangedByUser { get; set; }
        public virtual User? FromEmployee { get; set; }
        public virtual User? ToEmployee { get; set; }
    }
}
