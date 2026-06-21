using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Enquiry;

namespace RenewOne.API.Controllers.Enquiry
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnquiryController : ControllerBase
    {
        private readonly IEnquiryLogicService _enquiryLogicService;
        public EnquiryController(IEnquiryLogicService enquiryLogicService)
        {
            _enquiryLogicService = enquiryLogicService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnquiry(Guid id)
        {
            var result = await _enquiryLogicService.GetEnquiryAsync(id);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("get-all-enquiries")]
        public async Task<IActionResult> GetEnquiries(EnquiryFilter request)
        {

            var result = await _enquiryLogicService.GetEnquiriesAsync(request);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("get-my-enquiries")]
        public async Task<IActionResult> GetMyEnquiries(EnquiryFilter request)
        {

            var result = await _enquiryLogicService.GetMyEnquiriesAsync(request);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("dashboard/all")]
        public async Task<IActionResult> GetAllDashboard([FromQuery] DashboardFilter filter)
        {
            var result = await _enquiryLogicService.AllEnquiryDashboard(filter);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("dashboard/my")]
        public async Task<IActionResult> GetMyDashboard([FromQuery] DashboardFilter filter)
        {
            var result = await _enquiryLogicService.MyEnquiryDashboard(filter);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }
    }
}
