using System;
using System.Collections.Generic;
using System.Text;

namespace RenewOne.Application.DTOs.Request.Reports
{
    public sealed class EnquiryCancelledReportRow
    {
        public Guid EnquiryNumber { get; init; }

        public string CustomerName { get; init; } = string.Empty;

        public string CustomerNumber { get; init; } = string.Empty;

        public DateTime? CancellationDate { get; init; }

        public string? CancellationReason { get; init; }

        public string? Employee { get; init; }

        public string? Agency { get; init; }

        public string? InsuranceType { get; init; }
    }
}
