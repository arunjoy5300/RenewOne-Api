using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class Account : BaseEntity
    {
        public string AgentName { get; set; }
        public string? Code { get; set; }
        public Guid AgentClassId { get; set; }
        public string? Address { get; set; }
        public string? Pincode { get; set; }
        public string ContactNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public virtual AgentClass? AgentClass { get; set; }
        public virtual ICollection<User> Users { get; set; } = new HashSet<User>();
        public virtual ICollection<Enquiry> Enquiries { get; set; } = new HashSet<Enquiry>();
        public virtual Wallet? Wallet { get; set; }
    }
}
