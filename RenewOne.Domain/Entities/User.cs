using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class User : BaseEntity
    {
        public Guid? AccountId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string UserType { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual Account? Account { get; set; }
        public virtual ICollection<LoginHistory> LoginHistories { get; set; } = new HashSet<LoginHistory>();
        public virtual ICollection<Enquiry> CreatedEnquiries { get; set; } = new HashSet<Enquiry>();
        public virtual ICollection<Enquiry> AssignedEnquiries { get; set; } = new HashSet<Enquiry>();
    }
}
