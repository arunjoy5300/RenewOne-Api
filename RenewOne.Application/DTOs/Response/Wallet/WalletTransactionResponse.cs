using RenewOne.Domain.Enums;

namespace RenewOne.Application.DTOs.Response.Wallet
{
    public class WalletTransactionResponse
    {
        public Guid Id { get; set; }
        public WalletTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? ReferenceNo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
