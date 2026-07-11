using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;

namespace RenewOne.API.Controllers.AdminEntry
{
    [ApiController]
    [Route("api/insurance-types")]
    [Authorize]
    public class InsuranceTypeController : ControllerBase
    {
        private readonly IInsuranceTypeLogicService _logicService;

        public InsuranceTypeController(
            IInsuranceTypeLogicService logicService)
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
