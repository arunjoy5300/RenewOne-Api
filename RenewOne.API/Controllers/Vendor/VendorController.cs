using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Vendor;
using RenewOne.Application.Interfaces.LogicService.Vendors;

namespace RenewOne.API.Controllers.Vendor
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendorController : ControllerBase
    {
        private readonly IVendorLogicService _vendorService;
        public VendorController(IVendorLogicService vendorLogicService)
        {
            _vendorService = vendorLogicService;
        }
        [HttpPost]
        public async Task<IActionResult> Create(
        [FromBody] CreateVendorRequest request,
        CancellationToken ct)
        {
            var result = await _vendorService.CreateVendorAsync(request, ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            CancellationToken ct)
        {
            var result = await _vendorService.GetVendorsAsync(ct);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken ct)
        {
            var result = await _vendorService.GetVendorAsync(id, ct);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateVendorRequest request,
            CancellationToken ct)
        {
            var result = await _vendorService.UpdateVendorAsync(
                id, request, ct);

            return Ok(result);
        }

        [HttpPut("toggle/{id:guid}")]
        public async Task<IActionResult> ToggleStatus(
            Guid id,
            CancellationToken ct)
        {
            var result = await _vendorService.ToggleVendorStatusAsync(id, ct);
            return Ok(result);
        }
    }
}
