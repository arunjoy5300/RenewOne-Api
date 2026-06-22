namespace RenewOne.Application.DTOs.Response.Wallet
{
    public class WalletRechargeResponse
    {
        public string MerchantOrderId { get; set; } = string.Empty;
        public string PaymentSessionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
