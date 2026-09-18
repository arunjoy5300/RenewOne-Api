using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.Filters.BankAccount
{
    public class BankTransactionFilterRequest : PagedRequest
    {
        public string? Search { get; set; }               // Matches Description, Category, or Bank Account details
        public Guid? BankAccountId { get; set; }          // Specific bank account filter
        public string? TransactionType { get; set; }      // "Credit" or "Debit"
        public string? Category { get; set; }             // e.g. "PolicyPayment", "Salary", "Vendor"
        public Guid? UserId { get; set; }
        public Guid? VendorId { get; set; }
        public Guid? PolicyPaymentId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; }               // "date", "amount"
        public string? SortOrder { get; set; }            // "asc", "desc"
    }
}
