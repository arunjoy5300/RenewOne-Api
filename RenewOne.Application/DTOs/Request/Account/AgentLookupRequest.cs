using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.DTOs.Request.Account
{
    public class AgentLookupRequest : PagedRequest
    {
        public string? Search { get; set; }
    }
}
