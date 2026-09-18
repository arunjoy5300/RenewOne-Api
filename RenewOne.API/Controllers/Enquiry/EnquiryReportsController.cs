using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Request.Reports;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Domain.Reports.Enquiries.Models;

namespace RenewOne.API.Controllers.Enquiry
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class EnquiryReportsController : ControllerBase
    {
        private readonly IEnquiryReportDefinitionProvider _definitionProvider;
        private readonly IEnquiryReportLogicService _logicService;

        public EnquiryReportsController(
            IEnquiryReportDefinitionProvider definitionProvider,
            IEnquiryReportLogicService logicService)
        {
            _definitionProvider = definitionProvider;
            _logicService = logicService;
        }

        [HttpGet("definitions")]
        public ActionResult<IReadOnlyList<EnquiryReportDefinition>> GetDefinitions()
        {
            var definitions = _definitionProvider.GetDefinitions();

            return Ok(definitions);
        }
        [HttpPost("generate")]
        public async Task<ActionResult<EnquiryReportResult>> Generate([FromBody] EnquiryReportRequest request,CancellationToken cancellationToken)
        {
            var result = await _logicService.GenerateAsync(
                request,
                cancellationToken);

            return Ok(result);
        }
    }
}
