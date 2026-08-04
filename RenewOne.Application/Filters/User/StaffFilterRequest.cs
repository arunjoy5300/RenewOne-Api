using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.Filters.User
{
    public class StaffFilterRequest : PagedRequest
    {
        public string? Search { get; set; }
        public bool? Status { get; set; }
    }
}
