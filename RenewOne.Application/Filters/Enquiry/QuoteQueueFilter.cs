using RenewOne.Application.Common.Pagination;
using RenewOne.Domain.Enums;

namespace RenewOne.Application.Filters.Enquiry
{
    public class QuoteQueueFilter : PagedRequest
    {
        public QuoteQueueType Type { get; set; }
        public string? Search { get; set; }
        public Guid? InsuranceTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public Guid? AccountId { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }
        public bool SortDescending { get; set; } = true;
        public DateFilterType DateFilter { get; set; }
    }
}
