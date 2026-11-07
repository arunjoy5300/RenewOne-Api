using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class Ledger: BaseEntity
    {
        public Guid AccountId { get; set; }
        // Prepaid balance managed by admins and consumed by policy payments.
        public decimal AvailableBalance { get; set; }
        public bool IsActive { get; set; }
        public virtual Account Account { get; set; }
        public virtual ICollection<LedgerTransaction> LedgerTransactions { get; set; }
        = new HashSet<LedgerTransaction>();
    }
}
