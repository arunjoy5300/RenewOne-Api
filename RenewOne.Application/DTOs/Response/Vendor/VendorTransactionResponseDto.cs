namespace RenewOne.Application.DTOs.Response.Vendor
{
    public class VendorTransactionResponseDto
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public string? EnquiryId { get; set; }

        public string TransactionType { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }

        public string? ReferenceNumber { get; set; }
        public string? PaymentMode { get; set; }
        public string? Remarks { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
