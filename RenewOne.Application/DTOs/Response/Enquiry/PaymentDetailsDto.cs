namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class PaymentDetailsDto
    {
        public Guid EnquiryId { get; set; }

        public string PaymentMode { get; set; } = string.Empty;

        // Common
        public decimal Amount { get; set; }
        public string? Remarks { get; set; }

        // Ledger
        public string? AgentLedgerName { get; set; }
        public decimal? AgentTotalLimit { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? LedgerPaymentTime { get; set; }

        // Verification
        public string? VerifiedByName { get; set; }
        public DateTime? VerifiedAt { get; set; }

        // Bank Transfer
        public string? AccountName { get; set; }
        public string? UpiId { get; set; }
        public string? AccountNumber { get; set; }
        public string? IFSC { get; set; }
        public DateTime? PaymentDate { get; set; }
    }
}
