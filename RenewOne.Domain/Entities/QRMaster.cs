using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class QRMaster : BaseEntity
    {
        public string UpiId { get; set; }
        public string UpiName { get; set; }
        public Guid BankAccountId { get; set; }
        public bool IsActive { get; set; }
        public virtual BankAccount BankAccount { get; set; } = null!;
        public virtual ICollection<QRMapping> QRMappings { get; set; } = new HashSet<QRMapping>();
    }
}
