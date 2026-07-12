using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Account;
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
        [Authorize(Roles = "SuperAdmin,AdminEmployee")]
        [HttpPost("approve-agent/{accountId}")]
        [Authorize(Roles = "SuperAdmin,AdminStaff")]
        public async Task<IActionResult> ApproveAgent([FromRoute] Guid accountId)
        {
            var result = await _accountLogicService.ApproveAgentAsync(accountId);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin,AdminEmployee")]
        [HttpGet("pending-agents")]
        public async Task<IActionResult> GetPendingAgents([FromQuery] PendingAgentFilterRequest filter)
        {
            var result = await _accountLogicService.GetPendingAgentsAsync(filter);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("agents")]
        [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
        public async Task<IActionResult> GetAgents(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        {
            var result = await _accountLogicService.GetAgentsAsync(
                search,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
    }
}
