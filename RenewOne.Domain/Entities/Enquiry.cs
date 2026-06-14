using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class Enquiry : BaseEntity
    {
        public Guid TenantId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public Guid InsuranceTypeId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? AssignedEmployeeId { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? SelectedQuoteId { get; set; }
        public virtual Account? Tenant { get; set; }
        public virtual InsuranceType? InsuranceType { get; set; }
        public virtual VehicleEnquiryDetails? VehicleEnquiryDetails { get; set; }
        public virtual ICollection<Document> Documents { get; set; } = new HashSet<Document>();
        public virtual ICollection<AssignmentHistory> AssignmentHistories { get; set; } = new HashSet<AssignmentHistory>();
        public virtual ICollection<EnquiryQuote> EnquiryQuotes { get; set; } = new List<EnquiryQuote>();

    }
}
