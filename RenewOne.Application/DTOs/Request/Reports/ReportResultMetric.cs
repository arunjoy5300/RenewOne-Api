namespace RenewOne.Application.DTOs.Request.Reports
{
    public sealed class ReportResultMetric
    {
        public string Key { get; init; } = string.Empty;

        public string Label { get; init; } = string.Empty;

        public object? Value { get; init; }

        public string Format { get; init; } = string.Empty;
    }
}
