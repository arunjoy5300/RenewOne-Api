using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Account;
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
    }
}
