namespace RenewOne.Application.DTOs.Request.Reports
{
    public sealed class EnquiryStatusReportRow
    {
        public Guid EnquiryNumber { get; init; }

        public string CustomerName { get; init; } = string.Empty;

        public string CustomerNumber { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public DateTime CreatedAt { get; init; }

        public string? Employee { get; init; }

        public string? Agency { get; init; }

        public string? InsuranceType { get; init; }
    }
}
