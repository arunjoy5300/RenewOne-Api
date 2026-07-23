using RenewOne.Application.Common.Pagination;

namespace RenewOne.Application.DTOs.Response.Account
{
    public class AgentDashboardResponse
    {
        public AgentDashboardSummary Summary { get; set; } = new();

        public PagedResponse<AgentListItem> Agents { get; set; } = new();
    }
}
