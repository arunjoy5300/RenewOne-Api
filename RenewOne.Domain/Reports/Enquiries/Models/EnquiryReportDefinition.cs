using RenewOne.Domain.Enums;

namespace RenewOne.Domain.Reports.Enquiries.Models
{
    public sealed class EnquiryReportDefinition
    {
        public EnquiryReportType Type { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public IReadOnlyList<ReportFilterDefinition> Filters { get; init; } = [];
        public IReadOnlyList<ReportColumnDefinition> Columns { get; init; } = [];
        public IReadOnlyList<ReportMetricDefinition> Metrics { get; init; } = [];
    }
}
