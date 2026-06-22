using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class PaymentOrder : BaseEntity
    {
        public Guid AccountId { get; set; }
        public string Module { get; set; }
        public decimal Amount { get; set; }
        public string CashfreeOrderId { get; set; }
        public string CashfreePaymentSessionId { get; set; }
        public string CashfreePaymentId { get; set; }
        public string Status { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByIpAddress { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
