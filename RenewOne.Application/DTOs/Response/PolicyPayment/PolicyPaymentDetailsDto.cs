using System.Drawing;

namespace RenewOne.Application.DTOs.Response.PolicyPayment
{
    public class PolicyPaymentDetailsDto
    {
        public Guid PolicyPaymentId { get; set; }
        public string EnquiryId { get; set; }
        public string FullName { get; set; }
        public string VehicleNumber { get; set; }
        public string InsuranceCompanyName { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal FinalPayableAmount { get; set; }
        public string PaymentMode { get; set; }
        public string LedgerBalance { get; set; }
        public string TransactionId { get; set; }
        public string Remarks { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedOn { get; set; }

    }
}
