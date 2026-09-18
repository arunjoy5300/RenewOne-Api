using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class Vendor : BaseEntity
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public Guid? BankAccountId { get; set; }
        public bool IsActive { get; set; }
        public ICollection<VendorTransaction> VendorTransactions { get; set; } = new List<VendorTransaction>();
        public virtual BankAccount? BankAccount { get; set; }
    }
}
