using System.Drawing;

namespace RenewOne.Application.DTOs.Response.PolicyPayment
{
    public class PolicyPaymentDetailsDto
    {
        public Guid PolicyPaymentId { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public string EnquiryId { get; set; }
        public string FullName { get; set; }
        public string VehicleNumber { get; set; }
        public string InsuranceCompanyName { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal FinalPayableAmount { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMode { get; set; }
        public string? ReferenceNumber { get; set; }
        public string ManualWalletBalance { get; set; }
        public string? TransactionId { get; set; }
        public string? Remarks { get; set; }
        public string SubmittedBy { get; set; }
        public string SubmittedOn { get; set; }
        public Guid? BankAccountId { get; set; }
        public string? BankAccountName { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankBranch { get; set; }
        public string? BankIfscCode { get; set; }
        public string? AccountHolderName { get; set; }

    }
}
