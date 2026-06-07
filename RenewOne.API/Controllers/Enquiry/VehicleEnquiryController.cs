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
        //[Authorize(Roles = "SuperAdmin,Agent,Staff")]
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
    }
}
