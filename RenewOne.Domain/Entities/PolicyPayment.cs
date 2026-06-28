using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class PolicyPayment : BaseEntity
    {
        public Guid EnquiryId { get; set; }
        public Guid EnquiryQuoteId { get; set; }
        public Guid AccountId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMode { get; set; }
        public string Status { get; set; }
        public string? TransactionId { get; set; }
        public string? Remarks { get; set; }
        public Guid SubmittedByUserId { get; set; }
        public Guid? VerifiedbyUserId { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public virtual Enquiry Enquiry { get; set; }
        public virtual EnquiryQuote EnquiryQuote { get; set; } = null!;
        public virtual Account Account { get; set; } = null!;
        public virtual User SubmittedByUser { get; set; } = null!;
        public virtual User VerifiedByUser { get; set; }

    }
}
