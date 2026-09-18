namespace RenewOne.Application.DTOs.Response.BankAccount
{
    public class BankTransactionItemDto
    {
        public Guid Id { get; set; }
        public Guid BankAccountId { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string AccountNumberMasked { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // "Credit" | "Debit"
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? PolicyPaymentId { get; set; }
        public Guid? SalaryId { get; set; }
        public Guid? VendorId { get; set; }
        public Guid? UserId { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
