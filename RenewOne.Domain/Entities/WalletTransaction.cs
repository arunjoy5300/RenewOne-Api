using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public Guid? PaymentOrderId { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public string TransactionType { get; set; }
        public string? Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByIpAddress { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public string ReferenceNo { get; set; }
        public virtual Wallet Wallet { get; set; }
        public virtual PaymentOrder? PaymentOrder { get; set; }

    }
}
