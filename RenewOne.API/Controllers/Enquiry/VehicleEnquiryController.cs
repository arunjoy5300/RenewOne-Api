using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.Document;
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
        [Authorize()]
        public async Task<IActionResult> CreateVehicleEnquiry([FromBody] CreateVehicleEnquiry request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(Result<bool>.Failure("Please login again", 401));
            }

            var userId = Guid.Parse(userIdClaim);

            Guid accountId;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "SuperAdmin" ||
                role == "AdminEmployee" ||
                role == "Supervisor")
            {
                if (!request.AccountId.HasValue)
                {
                    return BadRequest(Result<bool>.Failure("AccountId is required for admin."));
                }

                accountId = request.AccountId.Value;
            }
            else
            {
                var accountIdClaim = User.FindFirst("AccountId")?.Value;

                if (string.IsNullOrEmpty(accountIdClaim))
                {
                    return Unauthorized(Result<bool>.Failure("No account information found."));
                }

                accountId = Guid.Parse(accountIdClaim);
            }

            var result = await _enquiryService.CreateVehicleEnquiryAsync(
                request,
                accountId,
                userId);

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
        [Authorize()]
        public async Task<IActionResult> SubmitVehicleEnquiry(SubmitVehicleEnquiry request, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(Result<bool>.Failure("Please login again.", 401));
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            Guid accountId;

            if (role == "SuperAdmin" ||
                role == "AdminEmployee" ||
                role == "Supervisor")
            {
                if (!request.AccountId.HasValue)
                {
                    return BadRequest(Result<bool>.Failure("AccountId is required for admin."));
                }

                accountId = request.AccountId.Value;
            }
            else
            {
                var accountIdClaim = User.FindFirst("AccountId")?.Value;

                if (string.IsNullOrEmpty(accountIdClaim))
                {
                    return Unauthorized(Result<bool>.Failure("No account information found. Please login again.", 401));
                }

                accountId = Guid.Parse(accountIdClaim);
            }

            var result = await _enquiryService.SubmitVehicleEnquiryAsync(
                request,
                accountId,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
        [Authorize()]
        [HttpPost("submit-policy-request")]
        public async Task<IActionResult> SubmitPolicyRequest(SubmitPolicyRequest request, CancellationToken cancellationToken)
        {
            var response = await _enquiryService.SubmitPolicyRequestAsync(request, cancellationToken);
            //if (!response.IsSuccess)
            //    return StatusCode(response.StatusCode, response);

            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("{enquiryId:guid}/payment-verification")]
        public async Task<IActionResult> GetPaymentDetails(Guid enquiryId)
        {
            var result = await _enquiryService
                .GetPaymentDetails(enquiryId);

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("{enquiryId:guid}/documents")]
        public async Task<IActionResult> ListUploadedDocuments(Guid enquiryId, CancellationToken cancellationToken)
        {
            var result = await _enquiryService.ListUploadedDocuments(enquiryId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{documentId:guid}")]
        public async Task<IActionResult> DeleteUploadedDocument(Guid documentId, CancellationToken cancellationToken)
        {
            var result = await _enquiryService.DeleteUploadedDocument(documentId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("replace-document")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ReplaceFile([FromForm] ReplaceFileDto replaceFileDto, CancellationToken cancellationToken)
        {
            var result = await _enquiryService.ReplaceFile(replaceFileDto, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("add-vehicle-quote")]
        public async Task<IActionResult> AddVehicleQuote([FromBody] AddVehicleQuoteRequest request, CancellationToken cancellationToken)
        {
            var result = await _enquiryService.AddVehicleQuote(request, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("quotes")]
        public async Task<IActionResult> ListVehicleQuotes([FromQuery] ListVehicleQuotesRequest request, CancellationToken cancellationToken)
        {
            var result = await _enquiryService.ListVehicleQuotes(request, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("{enquiryId:guid}/quotes/{quoteId:guid}/submit")]
        public async Task<IActionResult> SubmitVehicleQuoteByAdmin(Guid enquiryId, Guid quoteId, CancellationToken cancellationToken)
        {
            var result = await _enquiryService.SubmitVehicleQuoteByAdmin(enquiryId, quoteId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
    }
}
