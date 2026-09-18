using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Interfaces.LogicService.Vendors;

namespace RenewOne.API.Controllers.Vendor
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorTransactionController : ControllerBase
    {
        private readonly IVendorTransactionLogicService _vendorTransactionService;

        public VendorTransactionController(
            IVendorTransactionLogicService vendorTransactionService)
        {
            _vendorTransactionService = vendorTransactionService;
        }

        [HttpGet("vendor/{vendorId:guid}")]
        public async Task<IActionResult> GetVendorTransactions(
            Guid vendorId,
            CancellationToken ct)
        {
            var result = await _vendorTransactionService
                .GetVendorTransactionsAsync(vendorId, ct);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("vendor/{vendorId:guid}/balance")]
        public async Task<IActionResult> GetVendorBalance(
            Guid vendorId,
            CancellationToken ct)
        {
            var result = await _vendorTransactionService
                .GetVendorBalanceAsync(vendorId, ct);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken ct)
        {
            var result = await _vendorTransactionService
                .GetByIdAsync(id, ct);

            return StatusCode(result.StatusCode, result);
        }
    }
}
