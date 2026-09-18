using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Account;
using RenewOne.Application.DTOs.Response.Account;
using RenewOne.Application.Filters.Account;
using RenewOne.Application.Interfaces.LogicService.Accounts;

namespace RenewOne.API.Controllers.Accounts
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountLogicService _accountLogicService;
        public AccountController(IAccountLogicService accountLogicService)
        {
            _accountLogicService = accountLogicService;
        }
        [HttpPost("register-agent")]
        public async Task<IActionResult> RegisterAgent([FromBody] AgentSignupRequest signupRequest)
        {
            var result = await _accountLogicService.RegisterAgentAsync(signupRequest);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpPost("approve-agent/{accountId}")]
        public async Task<IActionResult> ApproveAgent([FromRoute] Guid accountId)
        {
            var result = await _accountLogicService.ApproveAgentAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("agents")]
        public async Task<IActionResult> GetAgents([FromQuery] AgentFilterRequest filter,CancellationToken ct)
        {
            var result = await _accountLogicService.GetAgentsAsync(filter, ct);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        [HttpGet("agent-lookup")]
        public async Task<IActionResult> GetAgentLookup([FromQuery] AgentLookupRequest request)
        {
            var result = await _accountLogicService.GetAgentLookupAsync(request);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("user-profile/{userId}")]
        public async Task<IActionResult> GetUserProfile([FromRoute] Guid userId)
        {
            var result = await _accountLogicService.GetUserProfileAsync(userId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/agent-details/{accountId}")]
        public async Task<IActionResult> GetAgentDetails([FromRoute] Guid accountId)
        {
            var result = await _accountLogicService.GetAgentDetailsByAccountIdAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("admin/agent-dashboard-overview/{accountId}")] //inner page
        public async Task<IActionResult> GetAgentDashboardOverview([FromRoute] Guid accountId)
        {
            var result = await _accountLogicService.GetAgentDashboardOverviewAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }
        #region Agent Staff Dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetAgentDropdown(CancellationToken cancellationToken = default)
        {
            var result = await _accountLogicService.GetAgentDropdownAsync(cancellationToken);
            return StatusCode(result.StatusCode, result);
        }
        #endregion
    }
}
