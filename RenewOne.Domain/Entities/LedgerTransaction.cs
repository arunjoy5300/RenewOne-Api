using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class LedgerTransaction : BaseEntity
    {
        public Guid LedgerId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public Guid? EnquiryId { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Remarks { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByIpAddress { get; set; }
        public virtual Ledger Ledger { get; set; }
    }
}
