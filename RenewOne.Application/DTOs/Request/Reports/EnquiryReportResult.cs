using RenewOne.Application.Common.Pagination;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.DTOs.Request.Reports
{
    public sealed class EnquiryReportResult
    {
        public EnquiryReportType ReportType { get; init; }

        public IReadOnlyList<ReportResultMetric> Metrics { get; init; }
            = [];

        public IReadOnlyList<ReportResultColumn> Columns { get; init; }
            = [];

        public PagedResponse<IReadOnlyDictionary<string, object?>> Data { get; init; }
            = new();
    }
}
