using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Application.Interfaces.LogicService.Dashboard;

namespace RenewOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardLogicService _dashboardLogicService;

        public DashboardController(IDashboardLogicService dashboardLogicService)
        {
            _dashboardLogicService = dashboardLogicService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetDashboardMetrics([FromQuery] DashboardFilter dashboardFilter, CancellationToken ct)
        {
            var result = await _dashboardLogicService.GetDashboardMetricsAsync(dashboardFilter, ct);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(Roles = "Agent,Staff")]
        [HttpGet("products")]
        public async Task<IActionResult> GetActiveDashboardProducts(CancellationToken ct)
        {
            var result = await _dashboardLogicService.GetActiveDashboardProductsAsync(ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
