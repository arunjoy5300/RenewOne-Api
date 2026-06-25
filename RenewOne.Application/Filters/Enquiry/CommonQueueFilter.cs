using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.Filters.Enquiry
{
    public class CommonQueueFilter : PagedRequest
    {
        public string? Search { get; set; }
        public Guid? InsuranceTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public bool SortDescending { get; set; } = true;
    }
}
