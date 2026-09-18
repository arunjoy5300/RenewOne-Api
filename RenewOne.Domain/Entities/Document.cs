using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class Document : BaseEntity
    {
        public Guid EnquiryId { get; set; }
        public string DocType { get; set; } = string.Empty;
        public string DocSide { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public Guid? UploadedUserId { get; set; }
        public string? UploadUserIpAddress { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual Enquiry? Enquiry { get; set; }
        public virtual User? UploadedUser { get; set; }
    }
}
