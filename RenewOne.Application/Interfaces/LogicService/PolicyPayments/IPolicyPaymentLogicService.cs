using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.PolicyPayment;
using RenewOne.Application.DTOs.Response.QRManaging;

namespace RenewOne.Application.Interfaces.LogicService.PolicyPayments
{
    public interface IPolicyPaymentLogicService
    {
        Task<Result<string>> ApprovePolicyPaymentAsync(ApprovePolicyPaymentRequest request,CancellationToken cancellationToken = default);
        Task<Result<string>> RejectPolicyPaymentAsync(RejectPolicyPaymentRequest request,CancellationToken cancellationToken = default);
        Task<Result<QRSelectionResponse>> GetQRSelectionAsync(Guid enquiryId,CancellationToken cancellationToken = default);
    }
}
