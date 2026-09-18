namespace RenewOne.Domain.Reports.Enquiries.Models
{
    public sealed class ReportFilterDefinition
    {
        public string Key { get; init; } = string.Empty;
        public string Label { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public bool Required { get; init; }
        public bool Multiple { get; init; }
    }
}
