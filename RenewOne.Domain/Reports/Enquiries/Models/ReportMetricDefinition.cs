namespace RenewOne.Domain.Reports.Enquiries.Models
{
    public sealed class ReportMetricDefinition
    {
        public string Key { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string Format { get; init; } = string.Empty;
    }
}
