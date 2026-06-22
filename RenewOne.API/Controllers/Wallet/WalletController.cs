using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Wallet;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Wallet;

namespace RenewOne.API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletlogicService _walletlogicService;
        private readonly ILogger<WalletController> _logger;
        public WalletController(
            IWalletlogicService walletlogicService,
            ILogger<WalletController> logger
            )
        {
            _walletlogicService = walletlogicService;
            _logger = logger;
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
        [Authorize]
        [HttpPost("recharge")]
        public async Task<IActionResult> RechargeWallet(WalletRechargeRequest request)
        {
            var result = await _walletlogicService.RechargeWalletAsync(request);

            return StatusCode(result.StatusCode, result);
        }
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            var result = await _walletlogicService.ProcessWebhookAsync(body, Request.Headers);

            return StatusCode(result.StatusCode, result);
        }
    }
}
