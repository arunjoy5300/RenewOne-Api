namespace RenewOne.Application.DTOs.Response.Enquiry
{
    public class PaymentVerificationResponse
    {
        public Guid PolicyPaymentId { get; set; }
        public Guid EnquiryId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerNumber { get; set; } = string.Empty;
        public string VehicleNumber { get; set; } = string.Empty;
        public decimal Premium { get; set; }
        public string PaymentMode { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public string SubmittedBy { get; set; } = string.Empty;
        public string? Remarks { get; set; }
    }
}
