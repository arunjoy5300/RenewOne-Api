using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.PricingMaster;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;

namespace RenewOne.API.Controllers.AdminEntry.PricingMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingMasterController : ControllerBase
    {
        private readonly IPricingMasterLogicService _pricingService;

        public PricingMasterController(IPricingMasterLogicService pricingService)
        {
            _pricingService = pricingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PricingMasterRequest request)
        {
            var result = await _pricingService.CreatePricingAsync(request);
            if (!result.IsSuccess) return BadRequest(result.Errors);

            return StatusCode(result.StatusCode, result.Message);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _pricingService.GetAllPricingAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _pricingService.GetPricingByIdAsync(id);
            if (data == null) return NotFound();

            return Ok(data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PricingMasterRequest request)
        {
            var result = await _pricingService.UpdatePricingAsync(id, request);
            if (!result.IsSuccess) return StatusCode(result.StatusCode, result.Errors);

            return Ok(result.Message);
        }
    }
}
