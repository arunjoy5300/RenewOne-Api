using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class PaymentOrder : BaseEntity
    {
        public Guid AccountId { get; set; }

        // WalletRecharge, Subscription, PolicyPayment, etc.
        public string Module { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "INR";

        // Your generated order id (e.g. RW-20260622-ABC123)
        public string MerchantOrderId { get; set; } = string.Empty;

        // Cashfree response
        public string? CashfreeOrderId { get; set; }

        public string? CashfreePaymentSessionId { get; set; }

        // Received after successful payment
        public string? CashfreePaymentId { get; set; }

        // ACTIVE / SUCCESS / FAILED / EXPIRED
        public string Status { get; set; } = "Pending";

        // Customer Details
        public string? CustomerId { get; set; }

        public string? CustomerName { get; set; }

        public string? CustomerEmail { get; set; }

        public string? CustomerPhone { get; set; }

        // Cashfree Meta
        public string? ReturnUrl { get; set; }

        public string? NotifyUrl { get; set; }

        public string? Notes { get; set; }

        public DateTime? ExpiresAt { get; set; }

        // Audit
        public Guid CreatedByUserId { get; set; }

        public string CreatedByIpAddress { get; set; } = string.Empty;

        public DateTime? CompletedAt { get; set; }

        // Optional - useful for debugging
        public string? RawResponse { get; set; }
    }
}