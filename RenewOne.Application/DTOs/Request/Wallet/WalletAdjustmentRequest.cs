using RenewOne.Domain.Enums;

namespace RenewOne.Application.DTOs.Request.Wallet
{
    public class WalletAdjustmentRequest
    {
        public Guid AccountId { get; set; }
        public WalletTransactionType TransactionType { get; set; } // Credit / Debit
        public decimal Amount { get; set; }
        public string Remarks { get; set; } = string.Empty;
    }
}
