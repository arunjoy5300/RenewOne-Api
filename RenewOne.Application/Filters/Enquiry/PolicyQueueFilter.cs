using RenewOne.Application.Common.Pagination;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.Filters.Enquiry
{
    public class PolicyQueueFilter : PagedRequest
    {
        public PolicyQueueType Type { get; set; }
        public string? Search { get; set; }
        public EnquiryStatus? Status { get; set; }
        public Guid? InsuranceTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public Guid? AccountId { get; set; }
        public DateFilterType? DateFilter { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public bool SortDescending { get; set; } = true;
    }
}
