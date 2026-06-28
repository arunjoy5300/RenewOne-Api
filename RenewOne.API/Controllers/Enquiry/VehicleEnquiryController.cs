using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Enquiry.VehicleEnquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry.VehicleEnquiry;
using System.Security.Claims;

namespace RenewOne.API.Controllers.Enquiry
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleEnquiryController : ControllerBase
    {
        private readonly IVehicleEnquiryLogicService _enquiryService;
        public VehicleEnquiryController(IVehicleEnquiryLogicService enquiryService)
        {
            _enquiryService = enquiryService;
        }

        [HttpPost("create-vehicle-enquiry")]
        [Authorize(Roles = "Agent,Staff")]
        public async Task<IActionResult> CreateVehicleEnquiry([FromBody] CreateVehicleEnquiry request)
        {
            // Extract tenant and user information from claims
            var tenantIdClaim = User.FindFirst("AccountId")?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(tenantIdClaim) || string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(Result<bool>.Failure("No tenant or user information found.Please login again", 401));
            }

            var tenantId = Guid.Parse(tenantIdClaim);
            var userId = Guid.Parse(userIdClaim);

            var result = await _enquiryService.CreateVehicleEnquiryAsync(request, tenantId, userId);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("upload-docs")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument([FromForm] UploadVehicleDocumentRequest request,CancellationToken cancellationToken)
        {
            var result =
                await _enquiryService.UploadDocumentAsync(
                    request,
                    cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpPost("submit-vehicle-enquiry")]
        [Authorize(Roles = "Agent,Staff")]
        public async Task<IActionResult> SubmitVehicleEnquiry(SubmitVehicleEnquiry request, CancellationToken cancellationToken)
        {
            var tenantIdClaim = User.FindFirst("AccountId")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim))
            {
                return Unauthorized(Result<bool>.Failure("No tenant or user information found.Please login again", 401));
            }

            var accountId = Guid.Parse(tenantIdClaim);

            var result =
                await _enquiryService.SubmitVehicleEnquiryAsync(
                    request,
                    accountId,
                    cancellationToken);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("submit-policy-request")]
        [Authorize(Roles ="Agent,Staff")]
        public async Task<IActionResult> SubmitPolicyRequest(SubmitPolicyRequest request, CancellationToken cancellationToken)
        {
            var response = await _enquiryService.SubmitPolicyRequestAsync(request, cancellationToken);
            if (!response.IsSuccess)
                return StatusCode(response.StatusCode, response);

            return StatusCode(response.StatusCode, response);
        }
    }
}
