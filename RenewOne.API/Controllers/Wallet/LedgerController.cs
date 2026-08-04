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
        [HttpPost("add-ledger-balance")]
        public async Task<IActionResult> AddLedgerBalance([FromQuery]Guid accountId,WalletRechargeRequest request)
        {
            var response = await _ledgerLogicService.AddLedgerAsync(request, accountId);
            return StatusCode(response.StatusCode, response);
        }
    }
}
