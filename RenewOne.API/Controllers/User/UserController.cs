using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.User;
using RenewOne.Application.Interfaces.LogicService.User;

namespace RenewOne.API.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private Guid CurrentUserAccountId => Guid.Parse(User.FindFirst("AccountId")?.Value ?? Guid.Empty.ToString());
        private readonly IUserLogicService _userLogicService;
        public UserController(IUserLogicService userLogicService)
        {
            _userLogicService = userLogicService;
        }

        [Authorize(Roles ="Agent")]
        [HttpPost("create-staff")]
        public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
        {
            var result = await _userLogicService.CreateStaffAsync(request, CurrentUserAccountId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Agent,Staff")]
        [HttpGet("my-staff")]
        public async Task<IActionResult> GetMyStaff()
        {
            var result = await _userLogicService.GetMyStaffAsync(CurrentUserAccountId);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Roles = "Agent")]
        [HttpPut("toggle-status/{id}")]
        public async Task<IActionResult> ToggleStatus(Guid id)
        {
            var result = await _userLogicService.ToggleStaffStatusAsync(id, CurrentUserAccountId);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("admin/create-internal-user")]
        [Authorize(Roles = "SuperAdmin,Supervisor")]
        public async Task<IActionResult> CreateInternalUser([FromBody] CreateInternalUserRequest request)
        {
            var result = await _userLogicService.CreateInternalUserAsync(request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
