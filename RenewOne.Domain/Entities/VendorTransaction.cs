using RenewOne.Domain.Common;
using RenewOne.Domain.Enums;

namespace RenewOne.Domain.Entities
{
    public class VendorTransaction: BaseEntity
    {
        public Guid VendorId { get; set; }
        public Guid EnquiryId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; } = 0;
        public decimal BalanceAfter { get; set; } = 0;
        public string ReferenceNumber { get; set; }
        public string PaymentMode { get; set; }
        public string Remarks { get; set; }
        public DateTime TransactionDate { get; set; }
        public Guid CreatedByUserId { get; set; }
        public virtual Vendor Vendor { get; set; }
    }
}
