using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Wallet;

namespace RenewOne.API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletlogicService _walletlogicService;
        public WalletController(IWalletlogicService walletlogicService)
        {
            _walletlogicService = walletlogicService;
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            var result = await _walletlogicService.GetWalletAsync();

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetWalletTransactions([FromQuery]WalletTransactionFilter filter)
        {
            var result = await _walletlogicService.GetTransactionsAsync(filter);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode, result);

            return StatusCode(result.StatusCode, result);
        }
    }
}
