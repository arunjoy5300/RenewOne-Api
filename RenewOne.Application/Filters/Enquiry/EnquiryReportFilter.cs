namespace RenewOne.Application.Filters.Enquiry
{
    public sealed class EnquiryReportFilter
    {
        public DateTime? FromDate { get; init; }

        public DateTime? ToDate { get; init; }

        public IReadOnlyList<string>? Statuses { get; init; }

        public IReadOnlyList<Guid>? EmployeeIds { get; init; }

        public IReadOnlyList<Guid>? AgencyIds { get; init; }

        public IReadOnlyList<Guid>? InsuranceCompanyIds { get; init; }

        public IReadOnlyList<Guid>? InsuranceTypeIds { get; init; }

        public IReadOnlyList<Guid>? VehicleTypeIds { get; init; }

        public IReadOnlyList<Guid>? CancellationReasonIds { get; init; }

        public IReadOnlyList<int>? PendingStageIds { get; init; }

        public IReadOnlyList<int>? MissingDocumentIds { get; init; }

        public IReadOnlyList<Guid>? CancelledByIds { get; init; }

        public string? EnquiryNumber { get; init; }

        public string? CustomerName { get; init; }

        public string? VehicleNumber { get; init; }

        public decimal? MinPremium { get; init; }

        public decimal? MaxPremium { get; init; }

        public decimal? MinProfit { get; init; }

        public decimal? MaxProfit { get; init; }

        public decimal? MinMargin { get; init; }

        public decimal? MaxMargin { get; init; }
        public DateTime? CancellationFromDate { get; init; }

        public DateTime? CancellationToDate { get; init; }

        public IReadOnlyList<string>? AgeingRanges { get; init; }

        public string? GroupBy { get; init; }
    }
}
