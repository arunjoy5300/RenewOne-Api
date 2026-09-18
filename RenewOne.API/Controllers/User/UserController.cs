using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Employees;
using RenewOne.Application.DTOs.Request.User;
using RenewOne.Application.Filters.User;
using RenewOne.Application.Interfaces.LogicService.User;

namespace RenewOne.API.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserLogicService _userLogicService;
        public UserController(IUserLogicService userLogicService)
        {
            _userLogicService = userLogicService;
        }

        [HttpPost("create-staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
        {
            var result = await _userLogicService.CreateStaffAsync(request, request.AccountId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("list-staff/{accountId}")]
        public async Task<IActionResult> GetMyStaff(Guid accountId)
        {
            var result = await _userLogicService.GetMyStaffAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("list-all-staffs/{accountId}")]
        public async Task<IActionResult> ListStaffWithFilters(Guid accountId, [FromQuery] StaffFilterRequest request)
        {
            var result = await _userLogicService.ListStaffWithFiltersAsync(accountId, request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{accountId}/toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(Guid accountId, Guid id)
        {
            var result = await _userLogicService.ToggleStaffStatusAsync(id, accountId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("admin/create-internal-user")]
        [Authorize(Roles = "SuperAdmin,Supervisor")]
        public async Task<IActionResult> CreateInternalUser([FromBody] CreateInternalUserRequest request)
        {
            var result = await _userLogicService.CreateInternalUserAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("admin/get-all-employees")]
        [Authorize(Roles = "SuperAdmin,Supervisor")]
        public async Task<IActionResult> GetAllEmployees([FromQuery] EmployeeListRequest request)
        {
            var result = await _userLogicService.GetAdminEmployee(request);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("admin/toggle-employee-status/{id}")]
        [Authorize(Roles = "SuperAdmin,Supervisor")]
        public async Task<IActionResult> ToggleEmployeeStatus(Guid id)
        {
            var result = await _userLogicService.ToggleAdminEmployeeStatusAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("admin/employee-dashboard")]
        [Authorize(Roles = "SuperAdmin,Supervisor")]
        public async Task<IActionResult> GetEmployeeDashboard()
        {
            var result = await _userLogicService.GetAdminStaffDashboardOverviewAsync();
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/employee-rights/{id}")]
        [Authorize(Roles = "SuperAdmin,Supervisor,AdminEmployee")]

        public async Task<IActionResult> GetEmployeeRights(Guid id)
        {
            var result = await _userLogicService.GetAdminEmployeeRightsAsync(id);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("admin/update-employee-rights")]
        [Authorize(Roles = "SuperAdmin,Supervisor")]

        public async Task<IActionResult> UpdateEmployeeRights([FromBody] UpdateEmployeeRightsRequest request)
        {
            var result = await _userLogicService.UpdateAdminEmployeeRightsAsync(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("admin/update-employee")]
        [Authorize(Roles = "SuperAdmin,Supervisor")]
        public async Task<IActionResult> UpdateEmployee([FromBody] UpdateAdminEmployeeRequest request)
        {
            var result = await _userLogicService.UpdateAdminEmployeeAsync(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
