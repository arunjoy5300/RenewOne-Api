using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;

namespace RenewOne.API.Controllers
{
    [ApiController]
    [Route("api/vehicle-types")]
    [Authorize]
    public class VehicleTypeController : ControllerBase
    {
        private readonly IVehicleTypeLogicService _logicService;

        public VehicleTypeController(
            IVehicleTypeLogicService logicService)
        {
            _logicService = logicService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            CancellationToken cancellationToken)
        {
            var result = await _logicService.GetAllAsync(
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.GetByIdAsync(
                id,
                cancellationToken);

            return StatusCode(result.StatusCode, result);
        }
    }
}