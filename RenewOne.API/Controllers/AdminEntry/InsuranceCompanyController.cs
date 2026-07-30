using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.Interfaces.LogicService.AdminEntry;

namespace RenewOne.API.Controllers.AdminEntry
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceCompanyController : ControllerBase
    {
        private readonly IInsuranceCompanyLogicService _insuranceCompanyLogicService;
        public InsuranceCompanyController(IInsuranceCompanyLogicService insuranceCompanyLogicService)
        {
            _insuranceCompanyLogicService = insuranceCompanyLogicService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAllInsuranceCompanies(CancellationToken cancellationToken = default)
        {
            var result = await _insuranceCompanyLogicService.GetAllInsuranceCompaniesAsync(cancellationToken);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
