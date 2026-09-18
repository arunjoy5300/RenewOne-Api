using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Request.Vendor;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Domain.Enums;

namespace RenewOne.API.Controllers.Enquiry
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EnquiryController : ControllerBase
    {
        private readonly IEnquiryLogicService _enquiryLogicService;
        public EnquiryController(IEnquiryLogicService enquiryLogicService)
        {
            _enquiryLogicService = enquiryLogicService;
        }

        [HttpGet("{id}")]
        [Authorize()]
        public async Task<IActionResult> GetEnquiry(Guid id)
        {
            var result = await _enquiryLogicService.GetEnquiryAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("get-all-enquiries")]
        [Authorize(Roles ="Agent,Staff")]
        public async Task<IActionResult> GetEnquiries(EnquiryFilter request)
        {

            var result = await _enquiryLogicService.GetEnquiriesAsync(request);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("get-my-enquiries")]
        [Authorize(Roles ="Agent,Staff")]
        public async Task<IActionResult> GetMyEnquiries(EnquiryFilter request)
        {

            var result = await _enquiryLogicService.GetMyEnquiriesAsync(request);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("dashboard/all")]
        [Authorize(Roles ="Agent,Staff")]
        public async Task<IActionResult> GetAllDashboard([FromQuery] DashboardFilter filter)
        {
            var result = await _enquiryLogicService.AllEnquiryDashboard(filter);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("dashboard/my")]
        [Authorize(Roles ="Agent,Staff")]
        public async Task<IActionResult> GetMyDashboard([FromQuery] DashboardFilter filter)
        {
            var result = await _enquiryLogicService.MyEnquiryDashboard(filter);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("admin/common-queue")]
        [Authorize(Roles ="SuperAdmin,Supervisor,AdminEmployee")]
        public async Task<IActionResult> GetCommonQueue([FromQuery] CommonQueueFilter filter,CancellationToken ct)
        {
            var result = await _enquiryLogicService.GetCommonQueueAsync(filter,ct);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{enquiryId:guid}/assign")]
        public async Task<IActionResult> Assign(
        Guid enquiryId,
        Guid employeeId,
        string? remarks,
        CancellationToken cancellationToken)
        {
            var result = await _enquiryLogicService.AssignAsync(
                enquiryId,
                employeeId,
                remarks,

                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{enquiryId:guid}/assignable-employees")]
        public async Task<IActionResult> GetAssignableEmployees(
        Guid enquiryId,
        CancellationToken cancellationToken)
        {
            var result = await _enquiryLogicService.GetAssignableEmployeesAsync(
                enquiryId,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/ledger-payment-details/{enquiryId:guid}")]
        public async Task<IActionResult> GetLedgerPaymentDetails(Guid enquiryId, CancellationToken cancellationToken)
        {
            var result = await _enquiryLogicService.GetEnquiryLedgerDetails(enquiryId, cancellationToken);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("admin/mytask")]
        public async Task<IActionResult> GetTask([FromQuery] EnquiryFilter filter,CancellationToken ct)
        {
            var result = await _enquiryLogicService.MyTaskEnquiries(filter,ct);
            return StatusCode(result.StatusCode, result);

        }

        [HttpGet("admin/myquotequeue")]
        public async Task<IActionResult> GetMyQuoteQueue([FromQuery] QuoteQueueFilter filter, CancellationToken ct)
        {
            var result = await _enquiryLogicService.MyQuoteQueue(filter, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("admin/mypolicyqueue")]
        public async Task<IActionResult> GetMyPolicyQueue([FromQuery] PolicyQueueFilter filter, CancellationToken ct)
        {
            var result = await _enquiryLogicService.MyPolicyQueue(filter, ct);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/draftqueue")]
        public async Task<IActionResult> DraftQueue([FromQuery] PolicyQueueFilter filter, CancellationToken ct)
        {
            var result = await _enquiryLogicService.DraftQueue(filter, ct);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/dashboard")]
        public async Task<IActionResult> GetAdminEnquiryDashboardAsync([FromQuery] EnquiryDashboardFilter filter, CancellationToken ct)
        {
            var result = await _enquiryLogicService.GetAdminEnquiryDashboardAsync(filter, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("admin/all-enquiries")]
        public async Task<IActionResult> GetAllAdminEnquiriesAsync([FromQuery] PolicyQueueFilter filter, CancellationToken ct)
        {
            var result = await _enquiryLogicService.GetAllAdminEnquiriesAsync(filter, ct);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/policy-summary/{enquiryId:guid}")]
        public async Task<IActionResult> GetPolicySummaryAsync(Guid enquiryId, CancellationToken ct)
        {
            var result = await _enquiryLogicService.GetPolicySummaryAsync(enquiryId, ct);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/quote-summary/{enquiryId:guid}")]
        public async Task<IActionResult> GetPaymentDetailsAsync(Guid enquiryId, CancellationToken ct)
        {
            var result = await _enquiryLogicService.GetPaymentDetailsAsync(enquiryId, ct);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/quote-payment-details/{enquiryId:guid}/{vendorId:guid}/{settlementMethod}")]
        public async Task<IActionResult> GetVendorSettlementDetailsAsync(Guid enquiryId,Guid vendorId,VendorSettlementMethod settlementMethod,CancellationToken ct)
        {
            var result = await _enquiryLogicService
                .GetVendorSettlementDetailsAsync(
                    enquiryId,
                    vendorId,
                    settlementMethod,
                    ct);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("admin/{enquiryId:guid}/policy-details")]
        public async Task<IActionResult> CreatePolicyDetails(Guid enquiryId,[FromBody] CreatePolicyDetailsRequest request,CancellationToken ct)
        {
            var result = await _enquiryLogicService
                .CreatePolicyDetailsAsync(
                    enquiryId,
                    request,
                    ct);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("admin/{enquiryId:guid}/policy-details")]
        public async Task<IActionResult> GetPolicyDetails(Guid enquiryId,CancellationToken ct)
        {
            var result = await _enquiryLogicService
                .GetPolicyDetailsAsync(enquiryId, ct);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("admin/vendor-settlement")]
        public async Task<IActionResult> SubmitVendorSettlement([FromBody] SubmitVendorSettlementRequest request,CancellationToken ct)
        {
            var result = await _enquiryLogicService
                .SubmitVendorSettlementAsync(request, ct);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("admin/vendor-settlement-status/{enquiryId:guid}")]
        public async Task<IActionResult> IsVendorSettlementCompleted(Guid enquiryId,CancellationToken ct)
        {
            var result = await _enquiryLogicService
                .IsVendorSettlementCompletedAsync(enquiryId, ct);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("admin/change-status")]
        public async Task<IActionResult> ChangeEnquiryStatus(ChangeEnquiryStatusRequest request,CancellationToken ct)
        {
            var result = await _enquiryLogicService
                .ChangeEnquiryStatusAsync(request, ct);

            return StatusCode(result.StatusCode, result);
        }
        #region Agent Enquiries ( Agency Owner )
        [HttpGet("agent/enquiries")]
        public async Task<IActionResult> GetAgentEnquiriesAsync([FromQuery] AgentEnquiryFilterDto filter, CancellationToken ct)
        {
            var result = await _enquiryLogicService.GetAgentEnquiriesAsync(filter, ct);
            return StatusCode(result.StatusCode, result);
        }
        #endregion

        #region Agent View - Policy Preview
        [HttpGet("agent/policy-preview/{enquiryId:guid}")]
        public async Task<IActionResult> GetAgentPolicyPreview(Guid enquiryId, CancellationToken ct)
        {
            var result = await _enquiryLogicService.GetAgentPolicyPreview(enquiryId, ct);
            return StatusCode(result.StatusCode, result);
        }
        #endregion
    }
}
