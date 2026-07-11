using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.Filters.QRMapping
{
    public class QRMappingFilter : PagedRequest
    {
        public Guid? InsuranceTypeId { get; set; }
        public Guid? VehicleTypeId { get; set; }
        public Guid? QRMasterId { get; set; } 
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public bool SortDescending { get; set; } = true;
    }
}
