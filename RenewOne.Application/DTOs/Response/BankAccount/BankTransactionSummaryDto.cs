using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.DTOs.Response.BankAccount
{
    public class BankTransactionSummaryDto
    {
        public decimal TotalCredited { get; set; }
        public decimal TotalDebited { get; set; }
        public decimal NetCashFlow => TotalCredited - TotalDebited;
        public int TotalTransactions { get; set; }
    }

    public class BankTransactionResponse
    {
        public BankTransactionSummaryDto Summary { get; set; } = new();
        public PagedResponse<BankTransactionItemDto> Transactions { get; set; } = new();
    }
}
