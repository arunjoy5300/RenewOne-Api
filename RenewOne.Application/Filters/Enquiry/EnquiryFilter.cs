using RenewOne.Application.Common.Pagination;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.Filters.Enquiry
{
    public class EnquiryFilter : PagedRequest
    {
        public string? Search { get; set; }

        public Guid? InsuranceTypeId { get; set; }

        public Guid? VehicleTypeId { get; set; }

        public PolicyType? PolicyType { get; set; }

        public EnquiryStatus? EnquiryStatus { get; set; }

        public Guid? AgentId { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public EnquirySortBy SortBy { get; set; } = EnquirySortBy.CreatedAt;

        public SortDirection SortDirection { get; set; } = SortDirection.Desc;
    }
}
