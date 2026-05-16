using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Interfaces.LogicService.Class;

namespace RenewOne.API.Controllers.AdminControls
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAgentClassLogicService _agentClassLogicService;
        public AdminController(IAgentClassLogicService agentClassLogicService)
        {
            _agentClassLogicService = agentClassLogicService;
        }
        [HttpGet("view-agentclass")]
        public async Task<IActionResult> GetAllAgentClasses()
        {
            var result = await _agentClassLogicService.GetAllAgentClasses();
            if (result.IsSuccess)
                return Ok(result);
            else if (result.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
                return NotFound(result);
            else
                return StatusCode((int)result.StatusCode, result);
        }
        [HttpGet("view-agentclass/{id}")]
        public async Task<IActionResult> GetAgentClassById(Guid id)
        {
            var result = await _agentClassLogicService.GetAgentClassById(id);
            if (result.IsSuccess)
                return Ok(result);
            else if (result.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
                return NotFound(result);
            else
                return StatusCode((int)result.StatusCode, result);
        }
    }
}
