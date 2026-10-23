using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenewOne.Application.DTOs.Request.Enquiry;
using RenewOne.Application.DTOs.Request.Reports;
using RenewOne.Application.Interfaces.LogicService.Enquiry;
using RenewOne.Application.Interfaces.Repositories.Enquiry;
using RenewOne.Application.Interfaces.Services.Reports;
using RenewOne.Domain.Enums;
using RenewOne.Domain.Reports.Enquiries.Models;

namespace RenewOne.API.Controllers.Enquiry
{
    [Authorize(Roles = "SuperAdmin,AdminEmployee,Supervisor")]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class EnquiryReportsController : ControllerBase
    {
        private readonly IEnquiryReportDefinitionProvider _definitionProvider;
        private readonly IEnquiryReportLogicService _logicService;
        private readonly IReportExportService _exportService;

        public EnquiryReportsController(
            IEnquiryReportDefinitionProvider definitionProvider,
            IEnquiryReportLogicService logicService,
            IReportExportService exportService)
        {
            _definitionProvider = definitionProvider;
            _logicService = logicService;
            _exportService = exportService;
        }

        [HttpGet("definitions")]
        public ActionResult<IReadOnlyList<EnquiryReportDefinition>> GetDefinitions()
        {
            var definitions = _definitionProvider.GetDefinitions();
            return Ok(definitions);
        }

        [HttpPost("generate")]
        public async Task<ActionResult<EnquiryReportResult>> Generate(
            [FromBody] EnquiryReportRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.GenerateAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export(
            [FromBody] EnquiryReportRequest request,
            [FromQuery] ReportExportFormat format,
            [FromQuery] string? title,
            CancellationToken cancellationToken)
        {
            var result = await _logicService.GenerateAsync(request, cancellationToken);
            var export = _exportService.Export(result, format, title);

            return File(export.FileBytes, export.ContentType, export.FileName);
        }
    }
}
