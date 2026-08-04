using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Common.Models;
using RenewOne.Application.DTOs.Request.PricingMaster;
using RenewOne.Application.Filters.Admin;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PricingMasterFilterRequest request)
        {
            var data = await _pricingService.GetAllPricingAsync(request);
            return StatusCode(data.StatusCode, data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var data = await _pricingService.GetPricingByIdAsync(id);
            return StatusCode(data.StatusCode, data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PricingMasterRequest request)
        {
            var result = await _pricingService.UpdatePricingAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }
    }
}
