using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class BankAccount : BaseEntity
    {
        public string AccountName { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string? UpiId { get; set; }
        public string? AccountHolderName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual ICollection<QRMaster> QRMasters { get; set; }
            = new HashSet<QRMaster>();
        public virtual ICollection<BankTransaction> BankTransactions { get; set; }
        = new HashSet<BankTransaction>();
    }
}
