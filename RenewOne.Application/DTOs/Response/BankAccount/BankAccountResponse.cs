namespace RenewOne.Application.DTOs.Response.BankAccount
{
    public class BankAccountResponse
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string AccountHolderName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string IFSCCode { get; set; } = string.Empty;
        public string? UpiId { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal LedgerTotalCredited { get; set; }
        public decimal LedgerTotalDebited { get; set; }
        public int LedgerTransactionCount { get; set; }
        public decimal LedgerDerivedBalance => OpeningBalance + LedgerTotalCredited - LedgerTotalDebited;
        public decimal BalanceVariance => LedgerDerivedBalance - CurrentBalance;
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
    }
}
