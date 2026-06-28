using System.Drawing;

namespace RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry
{
    public class SubmitPolicyRequest
    {
        public Guid QuoteId { get; set; }
        public Guid EnquiryId { get; set; }
        public bool UseLedger { get; set; }
        public string? TransactionId { get; set;}
        public string? Remarks { get; set; }
    }
}
