using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry;

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
        public async Task<IActionResult> GetCommonQueue([FromQuery] CommonQueueFilter filter)
        {
            var result = await _enquiryLogicService.GetCommonQueueAsync(filter);

            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "SuperAdmin,Supervisor,AdminEmployee")]
        [HttpGet("admin/common-queue/my")]
        public async Task<IActionResult> GetMyCommonQueue([FromQuery] CommonQueueFilter filter)
        {
            var result = await _enquiryLogicService.GetMyCommonQueueAsync(filter);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{enquiryId:guid}/assign")]
        public async Task<IActionResult> Assign(
        Guid enquiryId,
        Guid employeeId,
        string remarks,
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
    }
}
