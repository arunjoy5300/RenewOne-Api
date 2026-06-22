using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Wallet;
using RenewOne.Application.Interfaces.LogicService.Class;
using RenewOne.Application.Interfaces.LogicService.Wallet;

namespace RenewOne.API.Controllers.AdminControls
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        #region DI
        private readonly IAgentClassLogicService _agentClassLogicService;
        private readonly IWalletlogicService _walletlogicService;
        #endregion

        #region Constructor
        public AdminController(
            IAgentClassLogicService agentClassLogicService,
            IWalletlogicService walletlogicService)
        {
            _agentClassLogicService = agentClassLogicService;
            _walletlogicService = walletlogicService;
        }
        #endregion

        [HttpGet("view-agentclass")]
        public async Task<IActionResult> GetAllAgentClasses()
        {
            var result = await _agentClassLogicService.GetAllAgentClasses();
            if (result.IsSuccess)
                return Ok(result);
            else if (result.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
                return NotFound(result);
            else
                return StatusCode((int)result.StatusCode, result);
        }
        [HttpGet("view-agentclass/{id}")]
        public async Task<IActionResult> GetAgentClassById(Guid id)
        {
            var result = await _agentClassLogicService.GetAgentClassById(id);
            if (result.IsSuccess)
                return Ok(result);
            else if (result.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
                return NotFound(result);
            else
                return StatusCode((int)result.StatusCode, result);
        }
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("wallet/transactions")]
        public async Task<IActionResult> AdjustWallet(WalletAdjustmentRequest request)
        {
            var result = await _walletlogicService.AdjustWalletAsync(request);

            return StatusCode(result.StatusCode, result);
        }
    }
}
