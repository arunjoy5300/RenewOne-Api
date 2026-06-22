using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public Guid AccountId { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public virtual Account Account { get; set; }
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; }
        = new List<WalletTransaction>();
    }
}
