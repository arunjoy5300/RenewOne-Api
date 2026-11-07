using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Wallet;
using RenewOne.Application.Interfaces.LogicService.Ledger;

namespace RenewOne.API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerLogicService _ledgerLogicService;
        public LedgerController(ILedgerLogicService ledgerLogicService)
        {
            _ledgerLogicService = ledgerLogicService;
        }
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "SuperAdmin,Supervisor")]
        [HttpPost("adjust-manual-wallet")]
        public async Task<IActionResult> AdjustManualWallet([FromBody] WalletAdjustmentRequest request)
        {
            var response = await _ledgerLogicService.AdjustManualWalletAsync(request);
            return StatusCode(response.StatusCode, response);
        }
    }
}
