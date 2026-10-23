using RenewOne.Application.DTOs.Request.Reports;
using RenewOne.Application.DTOs.Response.Reports;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.Interfaces.Services.Reports
{
    public interface IReportExportService
    {
        ReportExportResult Export(EnquiryReportResult reportResult, ReportExportFormat format, string? customTitle = null);
    }
}
