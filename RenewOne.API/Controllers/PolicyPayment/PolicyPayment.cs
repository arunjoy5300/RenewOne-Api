using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.PolicyPayment;
using RenewOne.Application.Interfaces.LogicService.PolicyPayments;

namespace RenewOne.API.Controllers.PolicyPayment
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
    public class PolicyPayment : ControllerBase
    {
        private readonly IPolicyPaymentLogicService _policyPaymentLogicService;
        public PolicyPayment(IPolicyPaymentLogicService policyPaymentLogicService)
        {
            _policyPaymentLogicService = policyPaymentLogicService;
        }
        [HttpPost("approve")]
        public async Task<IActionResult> ApprovePolicyPayment([FromBody] ApprovePolicyPaymentRequest request)
        {
            var result = await _policyPaymentLogicService
                .ApprovePolicyPaymentAsync(request);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("reject")]
        public async Task<IActionResult> RejectPolicyPayment([FromBody] RejectPolicyPaymentRequest request)
        {
            var result = await _policyPaymentLogicService
                .RejectPolicyPaymentAsync(request);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{enquiryId:guid}/qr")]
        public async Task<IActionResult> GetQRDetails(
        Guid enquiryId,
        CancellationToken cancellationToken)
        {
            var result = await _policyPaymentLogicService.GetQRSelectionAsync(
                enquiryId,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
    }
}
