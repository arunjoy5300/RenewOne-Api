using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class Ledger: BaseEntity
    {
        public Guid AccountId { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public virtual Account Account { get; set; }
        public virtual ICollection<LedgerTransaction> LedgerTransactions { get; set; }
        = new HashSet<LedgerTransaction>();
    }
}
