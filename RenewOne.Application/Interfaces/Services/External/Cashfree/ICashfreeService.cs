using RenewOne.Application.DTOs.Response.Service;

namespace RenewOne.Application.Interfaces.Services.External.Cashfree
{
    public interface ICashfreeService
    {
        Task<CashfreeCreateOrderResponse> CreateOrderAsync(CashfreeCreateOrderRequest request);
        bool VerifyWebhookSignature(string payload,string timestamp,string signature);
    }
}
