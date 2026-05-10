using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class Document : BaseEntity
    {
        public Guid EnquiryId { get; set; }
        public string DocType { get; set; } = string.Empty; // RC, Policy etc.
        public bool DocSide { get; set; } // Front/Back
        public string Url { get; set; } = string.Empty;
        public virtual Enquiry? Enquiry { get; set; }
    }
}
