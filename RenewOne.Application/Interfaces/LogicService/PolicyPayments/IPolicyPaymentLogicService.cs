using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.PolicyPayment;

namespace RenewOne.Application.Interfaces.LogicService.PolicyPayments
{
    public interface IPolicyPaymentLogicService
    {
        Task<Result<string>> ApprovePolicyPaymentAsync(ApprovePolicyPaymentRequest request,CancellationToken cancellationToken = default);
        Task<Result<string>> RejectPolicyPaymentAsync(RejectPolicyPaymentRequest request,CancellationToken cancellationToken = default);
    }
}
