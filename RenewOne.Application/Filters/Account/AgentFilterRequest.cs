using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.Filters.Account
{
    public class AgentFilterRequest : PagedRequest
    {
        public string? Search { get; set; }

        // All, Active, Inactive, PendingApproval
        public string? Status { get; set; }

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }
    }
}
