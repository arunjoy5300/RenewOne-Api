using RenewOne.Application.Common.Pagination;
using RenewOne.Application.Filters.Enquiry;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.DTOs.Request.Enquiry
{
    public sealed class EnquiryReportRequest : PagedRequest
    {
        public EnquiryReportType ReportType { get; init; }
        public EnquiryReportFilter Filter { get; init; } = new();
        public bool ExportAll { get; init; }
    }
}
