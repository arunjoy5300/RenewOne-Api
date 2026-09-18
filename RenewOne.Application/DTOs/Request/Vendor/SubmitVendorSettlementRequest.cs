using RenewOne.Domain.Enums;

namespace RenewOne.Application.DTOs.Request.Vendor
{
    public class SubmitVendorSettlementRequest
    {
        public Guid EnquiryId { get; set; }
        public Guid VendorId { get; set; }
        public VendorSettlementMethod SettlementMethod { get; set; }
        public string? PaymentMode { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Remarks { get; set; }
        public Guid? BankAccountId { get; set; }
    }
}
